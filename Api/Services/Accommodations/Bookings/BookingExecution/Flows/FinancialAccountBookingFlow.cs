using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution.Flows
{
    public class FinancialAccountBookingFlow : IFinancialAccountBookingFlow
    {
        public FinancialAccountBookingFlow(IBookingRecordsManager bookingRecordsManager,
            IDateTimeProvider dateTimeProvider,
            IBookingPaymentService paymentService,
            IBookingEvaluationStorage bookingEvaluationStorage,
            IBookingRateChecker rateChecker,
            IBookingRequestExecutor requestExecutor)
        {
            _bookingRecordsManager = bookingRecordsManager;
            _dateTimeProvider = dateTimeProvider;
            _paymentService = paymentService;
            _bookingEvaluationStorage = bookingEvaluationStorage;
            _rateChecker = rateChecker;
            _requestExecutor = requestExecutor;
        }
        
        

        public Task<Result<AccommodationBookingInfo>> BookByAccount(AccommodationBookingRequest bookingRequest,
            AgentContext agentContext, string languageCode, string clientIp)
        {
            return GetCachedAvailability(bookingRequest)
                .Check(CheckRateRestrictions)
                .Map(RegisterBooking)
                .CheckIf(IsDeadlinePassed, ChargeMoney)
                .Map(SendSupplierRequest)
                .Bind(GetAccommodationBookingInfo);


            bool IsDeadlinePassed((Data.Bookings.Booking booking, BookingAvailabilityInfo) bookingInfo)
            {
                var payDueDate = bookingInfo.booking.DeadlineDate ?? bookingInfo.booking.CheckInDate;
                return payDueDate <= _dateTimeProvider.UtcToday().AddDays(-Infrastructure.Constants.Common.DaysBeforeDeadlineWhenPayForBooking);
            }


            async Task<Result<BookingAvailabilityInfo>> GetCachedAvailability(AccommodationBookingRequest bookingRequest)
                => await _bookingEvaluationStorage.Get(bookingRequest.SearchId,
                    bookingRequest.ResultId,
                    bookingRequest.RoomContractSetId);
            

            Task<Result> CheckRateRestrictions(BookingAvailabilityInfo availabilityInfo) 
                => _rateChecker.Check(bookingRequest, availabilityInfo, agentContext);
            
            
            async Task<(Data.Bookings.Booking, BookingAvailabilityInfo)> RegisterBooking(BookingAvailabilityInfo bookingAvailability)
            {
                var referenceCode = await _bookingRecordsManager.Register(bookingRequest, bookingAvailability, agentContext, languageCode);
                var (_, _, booking, _) = await _bookingRecordsManager.Get(referenceCode);
                return (booking, bookingAvailability);
            }


            async Task<Result> ChargeMoney((Data.Bookings.Booking, BookingAvailabilityInfo) bookingInfo)
            {
                var (booking, _) = bookingInfo;
                return await _paymentService.Charge(booking, agentContext.ToUserInfo());
            }


            async Task<Booking> SendSupplierRequest((Data.Bookings.Booking, BookingAvailabilityInfo) bookingInfo)
            {
                var (booking, availabilityInfo) = bookingInfo;
                return await _requestExecutor.Execute(bookingRequest, availabilityInfo.AvailabilityId, booking, languageCode);
            }


            Task<Result<AccommodationBookingInfo>> GetAccommodationBookingInfo(EdoContracts.Accommodations.Booking details)
                => _bookingRecordsManager.GetAccommodationBookingInfo(details.ReferenceCode, languageCode);
            
            
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
        
        
        
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingPaymentService _paymentService;
        private readonly IBookingEvaluationStorage _bookingEvaluationStorage;
        private readonly IBookingRateChecker _rateChecker;
        private readonly IBookingRequestExecutor _requestExecutor;
    }
}