using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Flows;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Api.Services.Mailing;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingRegistrationService : IBookingRegistrationService
    {
        public BookingRegistrationService(IBookingRecordsManager bookingRecordsManager,
            IBookingDocumentsService documentsService,
            IPaymentNotificationService notificationService,
            IBookingMailingService bookingMailingService,
            IDateTimeProvider dateTimeProvider,
            IAccountPaymentService accountPaymentService,
            ISupplierConnectorManager supplierConnectorManager,
            IBookingPaymentService paymentService,
            IBookingEvaluationStorage bookingEvaluationStorage,
            IBookingPaymentService bookingPaymentService,
            IBookingRequestStorage requestStorage,
            IBookingRateChecker rateChecker,
            IBookingRequestExecutor requestExecutor,
            ILogger<BookingRegistrationService> logger)
        {
            _bookingRecordsManager = bookingRecordsManager;
            _documentsService = documentsService;
            _notificationService = notificationService;
            _bookingMailingService = bookingMailingService;
            _dateTimeProvider = dateTimeProvider;
            _accountPaymentService = accountPaymentService;
            _supplierConnectorManager = supplierConnectorManager;
            _paymentService = paymentService;
            _bookingEvaluationStorage = bookingEvaluationStorage;
            _bookingPaymentService = bookingPaymentService;
            _requestStorage = requestStorage;
            _rateChecker = rateChecker;
            _requestExecutor = requestExecutor;
            _logger = logger;
        }
        
        
        public async Task<Result<string, ProblemDetails>> Register(AccommodationBookingRequest bookingRequest, AgentContext agentContext, string languageCode)
        {
            return await GetCachedAvailability(bookingRequest)
                .Check(CheckRateRestrictions)
                .Map(Register);


            Task<Result<Unit, ProblemDetails>> CheckRateRestrictions(BookingAvailabilityInfo availabilityInfo) 
                => _rateChecker.Check(bookingRequest, availabilityInfo, agentContext).ToResultWithProblemDetails();


            async Task<string> Register(BookingAvailabilityInfo bookingAvailability)
            {
                var referenceCode = await _bookingRecordsManager.Register(bookingRequest, bookingAvailability, agentContext, languageCode);
                await _requestStorage.Set(referenceCode, (bookingRequest, bookingAvailability.AvailabilityId));
                return referenceCode;
            }


            // TODO NIJO-1135: Revert logging in further refactoring steps
            // Result<string, ProblemDetails> WriteLog(Result<string, ProblemDetails> result)
            //     => LoggerUtils.WriteLogByResult(result,
            //         () => _logger.LogBookingRegistrationSuccess($"Successfully registered a booking with reference code: '{result.Value}'"),
            //         () => _logger.LogBookingRegistrationFailure($"Failed to register a booking. AvailabilityId: '{availabilityId}'. " +
            //             $"Itinerary number: {bookingRequest.ItineraryNumber}. Passenger name: {bookingRequest.MainPassengerName}. Error: {result.Error.Detail}"));
        }
        
        public async Task<Result<AccommodationBookingInfo, ProblemDetails>> Finalize(string referenceCode, AgentContext agentContext, string languageCode)
        {
            var (_, isGetBookingFailure, booking, getBookingError) = await GetAgentsBooking()
                .Ensure(b => agentContext.AgencyId == b.AgencyId, ProblemDetailsBuilder.Build("The booking does not belong to your current agency"))
                .Bind(CheckBookingIsPaid)
                .OnFailure(WriteLogFailure);

            if (isGetBookingFailure)
                return Result.Failure<AccommodationBookingInfo, ProblemDetails>(getBookingError);
            

            return await GetRequestInfo(referenceCode)
                .Bind(SendSupplierRequest)
                .Bind(CaptureMoneyIfDeadlinePassed)
                .OnFailure(VoidMoneyAndCancelBooking)
                .Bind(NotifyOnCreditCardPayment)
                .Bind(GetAccommodationBookingInfo)
                .Finally(WriteLog);

            
            Task<Result<(AccommodationBookingRequest, string), ProblemDetails>> GetRequestInfo(string referenceCode) => _requestStorage.Get(referenceCode).ToResultWithProblemDetails();

            
            Task<Result<EdoContracts.Accommodations.Booking, ProblemDetails>> SendSupplierRequest((AccommodationBookingRequest request, string availabilityId) requestInfo)
            {
                var (request, availabilityId) = requestInfo;
                return _requestExecutor.Execute(request, availabilityId, booking, languageCode)
                    .ToResultWithProblemDetails();
            }


            Task<Result<Data.Bookings.Booking, ProblemDetails>> GetAgentsBooking()
                => _bookingRecordsManager.GetAgentsBooking(referenceCode, agentContext).ToResultWithProblemDetails();


            Result<Data.Bookings.Booking, ProblemDetails> CheckBookingIsPaid(Data.Bookings.Booking bookingFromPipe)
            {
                if (bookingFromPipe.PaymentStatus == BookingPaymentStatuses.NotPaid)
                {
                    _logger.LogBookingFinalizationPaymentFailure($"The booking with reference code: '{referenceCode}' hasn't been paid");
                    return ProblemDetailsBuilder.Fail<Data.Bookings.Booking>("The booking hasn't been paid");
                }

                return bookingFromPipe;
            }


            async Task<Result<EdoContracts.Accommodations.Booking, ProblemDetails>> CaptureMoneyIfDeadlinePassed(EdoContracts.Accommodations.Booking bookingInPipeline)
            {
                var daysBeforeDeadline = Infrastructure.Constants.Common.DaysBeforeDeadlineWhenPayForBooking;
                var now = _dateTimeProvider.UtcNow();

                var deadlinePassed = booking.CheckInDate <= now.AddDays(daysBeforeDeadline)
                    || (booking.DeadlineDate.HasValue && booking.DeadlineDate.Value.Date <= now.AddDays(daysBeforeDeadline));

                if (!deadlinePassed)
                    return bookingInPipeline;

                var (_, isPaymentFailure, _, paymentError) = await _bookingPaymentService.Capture(booking, agentContext.ToUserInfo());
                if (isPaymentFailure)
                    return ProblemDetailsBuilder.Fail<EdoContracts.Accommodations.Booking>(paymentError);

                return bookingInPipeline;
            }


            Task VoidMoneyAndCancelBooking(ProblemDetails problemDetails) => this.VoidMoneyAndCancelBooking(booking, agentContext);


            async Task<Result<Booking, ProblemDetails>> NotifyOnCreditCardPayment(Booking details)
            {
                await _bookingMailingService.SendCreditCardPaymentNotifications(details.ReferenceCode);
                return details;
            }


            Task<Result<AccommodationBookingInfo, ProblemDetails>> GetAccommodationBookingInfo(Booking details)
                => _bookingRecordsManager.GetAccommodationBookingInfo(details.ReferenceCode, languageCode)
                    .ToResultWithProblemDetails();


            void WriteLogFailure(ProblemDetails problemDetails)
                => _logger.LogBookingByAccountFailure($"Failed to finalize a booking with reference code: '{referenceCode}'. Error: {problemDetails.Detail}");


            Result<T, ProblemDetails> WriteLog<T>(Result<T, ProblemDetails> result)
                => LoggerUtils.WriteLogByResult(result,
                    () => _logger.LogBookingFinalizationSuccess($"Successfully finalized a booking with reference code: '{referenceCode}'"),
                    () => _logger.LogBookingFinalizationFailure(
                        $"Failed to finalize a booking with reference code: '{referenceCode}'. Error: {result.Error.Detail}"));
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
                .OnFailure(VoidMoneyAndCancelBooking)
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


            Task<Result<Booking, ProblemDetails>> SendSupplierRequest(AccommodationBookingRequest bookingRequest, string availabilityId, Data.Bookings.Booking booking, string languageCode)
                => _requestExecutor.Execute(bookingRequest, availabilityId, booking, languageCode).ToResultWithProblemDetails();


            Task VoidMoneyAndCancelBooking(ProblemDetails problemDetails) 
                => this.VoidMoneyAndCancelBooking(booking, agentContext);

            
            async Task<Result<EdoContracts.Accommodations.Booking, ProblemDetails>> SendReceiptIfPaymentMade(EdoContracts.Accommodations.Booking details)
                => wasPaymentMade
                    ? await SendReceipt(details, booking, agentContext)
                    : details;


            Task<Result<AccommodationBookingInfo, ProblemDetails>> GetAccommodationBookingInfo(EdoContracts.Accommodations.Booking details)
                => _bookingRecordsManager.GetAccommodationBookingInfo(details.ReferenceCode, languageCode)
                    .ToResultWithProblemDetails();
            

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
        
        
        private async Task<Result<EdoContracts.Accommodations.Booking, ProblemDetails>> SendReceipt(EdoContracts.Accommodations.Booking details, Data.Bookings.Booking booking, AgentContext agentContext)
        {
            var (_, isReceiptFailure, receiptInfo, receiptError) = await _documentsService.GenerateReceipt(booking.Id, agentContext.AgentId);
            if (isReceiptFailure)
                return ProblemDetailsBuilder.Fail<EdoContracts.Accommodations.Booking>(receiptError);

            await _notificationService.SendReceiptToCustomer(receiptInfo, agentContext.Email);
            return Result.Success<EdoContracts.Accommodations.Booking, ProblemDetails>(details);
        }


        private async Task VoidMoneyAndCancelBooking(Data.Bookings.Booking booking, AgentContext agentContext)
        {
            var (_, isFailure, _, error) = await _supplierConnectorManager.Get(booking.Supplier).CancelBooking(booking.ReferenceCode);
            if (isFailure)
            {
                _logger.LogBookingCancelFailure(
                    $"Failed to cancel a booking with reference code '{booking.ReferenceCode}': [{error.Status}] {error.Detail}");

                // We'll refund money only if the booking cancellation was succeeded on supplier
                return;
            }

            var (_, voidOrRefundFailure, voidOrRefundError) = await _paymentService.VoidOrRefund(booking, agentContext.ToUserInfo());
            if (voidOrRefundFailure)
                _logger.LogBookingCancelFailure($"Failure during cancellation of a booking with reference code '{booking.ReferenceCode}':" +
                    $"failed to void or refund money. Error: {voidOrRefundError}");
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
        private readonly IBookingMailingService _bookingMailingService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAccountPaymentService _accountPaymentService;
        private readonly ISupplierConnectorManager _supplierConnectorManager;
        private readonly IBookingPaymentService _paymentService;
        private readonly IBookingEvaluationStorage _bookingEvaluationStorage;
        private readonly IBookingPaymentService _bookingPaymentService;
        private readonly IBookingRequestStorage _requestStorage;
        private readonly IBookingRateChecker _rateChecker;
        private readonly IBookingRequestExecutor _requestExecutor;
        private readonly ILogger<BookingRegistrationService> _logger;
    }
}