using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.Mailing;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution.Flows
{
    public class BankCreditCardBookingFlow : IBankCreditCardBookingFlow
    {
        public BankCreditCardBookingFlow(IBookingRequestStorage requestStorage,
            IBookingRateChecker rateChecker,
            IBookingRecordsManager bookingRecordsManager,
            IBookingMailingService bookingMailingService,
            IBookingRequestExecutor requestExecutor,
            IBookingEvaluationStorage evaluationStorage,
            IBookingCreditCardPaymentService creditCardPaymentService,
            IDateTimeProvider dateTimeProvider,
            ILogger<BankCreditCardBookingFlow> logger)
        {
            _requestStorage = requestStorage;
            _rateChecker = rateChecker;
            _bookingRecordsManager = bookingRecordsManager;
            _bookingMailingService = bookingMailingService;
            _requestExecutor = requestExecutor;
            _evaluationStorage = evaluationStorage;
            _creditCardPaymentService = creditCardPaymentService;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }
        
        
        public async Task<Result<string>> Register(AccommodationBookingRequest bookingRequest, AgentContext agentContext, string languageCode)
        {
            return await GetCachedAvailability(bookingRequest)
                .Check(CheckRateRestrictions)
                .Map(Register);


            async Task<Result<BookingAvailabilityInfo>> GetCachedAvailability(AccommodationBookingRequest bookingRequest)
                => await _evaluationStorage.Get(bookingRequest.SearchId, bookingRequest.ResultId, bookingRequest.RoomContractSetId);

                
            Task<Result> CheckRateRestrictions(BookingAvailabilityInfo availabilityInfo) 
                => _rateChecker.Check(bookingRequest, availabilityInfo, agentContext);


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
        
        
        public async Task<Result<AccommodationBookingInfo>> Finalize(string referenceCode, AgentContext agentContext, string languageCode)
        {
            return await GetBooking()
                .Check(CheckBookingIsPaid)
                .CheckIf(IsDeadlinePassed, CaptureMoney)
                .Bind(SendSupplierRequest)
                .Bind(NotifyPaymentReceived)
                .Bind(GetAccommodationBookingInfo);

            
            Task<Result<Data.Bookings.Booking>> GetBooking()
                => _bookingRecordsManager.GetAgentsBooking(referenceCode, agentContext);
            
            
            Result CheckBookingIsPaid(Data.Bookings.Booking bookingFromPipe)
            {
                if (bookingFromPipe.PaymentStatus == BookingPaymentStatuses.NotPaid)
                {
                    _logger.LogBookingFinalizationPaymentFailure($"The booking with reference code: '{referenceCode}' hasn't been paid");
                    return Result.Failure<Data.Bookings.Booking>("The booking hasn't been paid");
                }

                return Result.Success();
            }
            
            
            bool IsDeadlinePassed(Booking booking)
            {
                var payDueDate = booking.DeadlineDate ?? booking.CheckInDate;
                return payDueDate <= _dateTimeProvider.UtcToday().AddDays(-Infrastructure.Constants.Common.DaysBeforeDeadlineWhenPayForBooking);
            }


            async Task<Result> CaptureMoney(Booking booking) 
                => await _creditCardPaymentService.Capture(booking, agentContext.ToUserInfo());
            

            async Task<Result<EdoContracts.Accommodations.Booking>> SendSupplierRequest(Data.Bookings.Booking booking)
            {
                var (_, isFailure, requestInfo, error) = await _requestStorage.Get(booking.ReferenceCode);
                if(isFailure)
                    return Result.Failure<EdoContracts.Accommodations.Booking>(error);

                var (request, availabilityId) = requestInfo;
                return await _requestExecutor.Execute(request, availabilityId, booking, agentContext, languageCode);
            }


            async Task<Result<EdoContracts.Accommodations.Booking>> NotifyPaymentReceived(EdoContracts.Accommodations.Booking details)
            {
                await _bookingMailingService.SendCreditCardPaymentNotifications(details.ReferenceCode);
                return details;
            }


            Task<Result<AccommodationBookingInfo>> GetAccommodationBookingInfo(EdoContracts.Accommodations.Booking details)
                => _bookingRecordsManager.GetAccommodationBookingInfo(details.ReferenceCode, languageCode);
        }
        
        
        private readonly IBookingRequestStorage _requestStorage;
        private readonly IBookingRateChecker _rateChecker;
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly IBookingMailingService _bookingMailingService;
        private readonly IBookingRequestExecutor _requestExecutor;
        private readonly IBookingEvaluationStorage _evaluationStorage;
        private readonly IBookingCreditCardPaymentService _creditCardPaymentService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<BankCreditCardBookingFlow> _logger;
    }
}