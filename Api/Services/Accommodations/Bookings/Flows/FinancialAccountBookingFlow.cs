using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Flows
{
    public class FinancialAccountBookingFlow : IFinancialAccountBookingFlow
    {
        public FinancialAccountBookingFlow(IBookingRecordsManager bookingRecordsManager,
            IBookingDocumentsService documentsService,
            IPaymentNotificationService notificationService,
            IDateTimeProvider dateTimeProvider,
            IAccountPaymentService accountPaymentService,
            IBookingEvaluationStorage bookingEvaluationStorage,
            IBookingRateChecker rateChecker,
            IBookingRequestExecutor requestExecutor)
        {
            _bookingRecordsManager = bookingRecordsManager;
            _documentsService = documentsService;
            _notificationService = notificationService;
            _dateTimeProvider = dateTimeProvider;
            _accountPaymentService = accountPaymentService;
            _bookingEvaluationStorage = bookingEvaluationStorage;
            _rateChecker = rateChecker;
            _requestExecutor = requestExecutor;
        }
        
        

        public async Task<Result<AccommodationBookingInfo, ProblemDetails>> BookByAccount(AccommodationBookingRequest bookingRequest,
            AgentContext agentContext, string languageCode, string clientIp)
        {
            var wasPaymentMade = false;
            var (_, isFailure, availabilityInfo, error) = await GetCachedAvailability(bookingRequest);
            if (isFailure)
                return Result.Failure<AccommodationBookingInfo, ProblemDetails>(error);

            // TODO NIJO-1135 Remove lots of code duplication in account and card purchase booking
            var (_, isRegisterFailure, booking, registerError) = await GetCachedAvailability(bookingRequest)
                .Check(CheckRateRestrictions)
                .Map(RegisterBooking)
                .Bind(GetBooking)
                .Bind(PayUsingAccountIfDeadlinePassed);

            if (isRegisterFailure)
                return Result.Failure<AccommodationBookingInfo, ProblemDetails>(registerError);

            return await SendSupplierRequest(bookingRequest, availabilityInfo.AvailabilityId, booking, languageCode)
                .Bind(SendReceiptIfPaymentMade)
                .Bind(GetAccommodationBookingInfo);


            Task<Result<Unit, ProblemDetails>> CheckRateRestrictions(BookingAvailabilityInfo availabilityInfo) 
                => _rateChecker.Check(bookingRequest, availabilityInfo, agentContext).ToResultWithProblemDetails();
            
            
            Task<string> RegisterBooking(BookingAvailabilityInfo bookingAvailability) 
                => _bookingRecordsManager.Register(bookingRequest, bookingAvailability, agentContext, languageCode);


            async Task<Result<Data.Bookings.Booking, ProblemDetails>> GetBooking(string referenceCode)
                => await _bookingRecordsManager.Get(referenceCode).ToResultWithProblemDetails();


            async Task<Result<Data.Bookings.Booking, ProblemDetails>> PayUsingAccountIfDeadlinePassed(Data.Bookings.Booking bookingInPipeline)
            {
                var daysBeforeDeadline = Infrastructure.Constants.Common.DaysBeforeDeadlineWhenPayForBooking;
                var now = _dateTimeProvider.UtcNow();
                var availabilityDeadline = availabilityInfo.RoomContractSet.Deadline.Date;

                var deadlinePassed = availabilityInfo.CheckInDate <= now.AddDays(daysBeforeDeadline)
                    || (availabilityDeadline.HasValue && availabilityDeadline <= now.AddDays(daysBeforeDeadline));

                if (!deadlinePassed)
                    return bookingInPipeline;

                var (_, isPaymentFailure, _, paymentError) = await _accountPaymentService.Charge(bookingInPipeline, agentContext, clientIp);
                if (isPaymentFailure)
                    return ProblemDetailsBuilder.Fail<Data.Bookings.Booking>(paymentError);

                wasPaymentMade = true;
                return bookingInPipeline;
            }


            async Task<Result<Booking, ProblemDetails>> SendSupplierRequest(AccommodationBookingRequest bookingRequest, string availabilityId, Data.Bookings.Booking booking, string languageCode) 
                => await _requestExecutor.Execute(bookingRequest, availabilityId, booking, languageCode);


            async Task<Result<EdoContracts.Accommodations.Booking, ProblemDetails>> SendReceiptIfPaymentMade(EdoContracts.Accommodations.Booking details)
                => wasPaymentMade
                    ? await SendReceipt(details, booking, agentContext)
                    : details;


            Task<Result<AccommodationBookingInfo, ProblemDetails>> GetAccommodationBookingInfo(EdoContracts.Accommodations.Booking details)
                => _bookingRecordsManager.GetAccommodationBookingInfo(details.ReferenceCode, languageCode)
                    .ToResultWithProblemDetails();
            
            
            async Task<Result<EdoContracts.Accommodations.Booking, ProblemDetails>> SendReceipt(EdoContracts.Accommodations.Booking details, Data.Bookings.Booking booking, AgentContext agentContext)
            {
                var (_, isReceiptFailure, receiptInfo, receiptError) = await _documentsService.GenerateReceipt(booking.Id, agentContext.AgentId);
                if (isReceiptFailure)
                    return ProblemDetailsBuilder.Fail<EdoContracts.Accommodations.Booking>(receiptError);

                await _notificationService.SendReceiptToCustomer(receiptInfo, agentContext.Email);
                return details;
            }
            

            // TODO NIJO-1135: Revert logging in further refactoring steps
            // void WriteLogFailure(ProblemDetails problemDetails)
            //     => _logger.LogBookingByAccountFailure($"Failed to book using account. Reference code: '{referenceCode}'. Error: {problemDetails.Detail}");
            //
            //
            // Result<T, ProblemDetails> WriteLog<T>(Result<T, ProblemDetails> result)
            //     => LoggerUtils.WriteLogByResult(result,
            //         () => _logger.LogBookingFinalizationSuccess($"Successfully booked using account. Reference code: '{booking.ReferenceCode}'"),
            //         () => _logger.LogBookingFinalizationFailure(
            //             $"Failed to book using account. Reference code: '{booking.ReferenceCode}'. Error: {result.Error.Detail}"));
        }
        
        
        private async Task<Result<BookingAvailabilityInfo, ProblemDetails>> GetCachedAvailability(
            AccommodationBookingRequest bookingRequest)
            => await _bookingEvaluationStorage.Get(bookingRequest.SearchId,
                    bookingRequest.ResultId,
                    bookingRequest.RoomContractSetId)
                .ToResultWithProblemDetails();
        
        
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly IBookingDocumentsService _documentsService;
        private readonly IPaymentNotificationService _notificationService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAccountPaymentService _accountPaymentService;
        private readonly IBookingEvaluationStorage _bookingEvaluationStorage;
        private readonly IBookingRateChecker _rateChecker;
        private readonly IBookingRequestExecutor _requestExecutor;
    }
}