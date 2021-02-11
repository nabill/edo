using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution.Flows
{
    public class FinancialAccountBookingFlow : IFinancialAccountBookingFlow
    {
        public FinancialAccountBookingFlow(IBookingRecordManager bookingRecordManager,
            IDateTimeProvider dateTimeProvider,
            IBookingAccountPaymentService accountPaymentService,
            IBookingEvaluationStorage bookingEvaluationStorage,
            IBookingRateChecker rateChecker,
            IBookingDocumentsService documentsService,
            IBookingInfoService bookingInfoService,
            IBookingRequestExecutor requestExecutor)
        {
            _bookingRecordManager = bookingRecordManager;
            _dateTimeProvider = dateTimeProvider;
            _accountPaymentService = accountPaymentService;
            _bookingEvaluationStorage = bookingEvaluationStorage;
            _rateChecker = rateChecker;
            _documentsService = documentsService;
            _bookingInfoService = bookingInfoService;
            _requestExecutor = requestExecutor;
        }
        
        

        public Task<Result<AccommodationBookingInfo>> BookByAccount(AccommodationBookingRequest bookingRequest,
            AgentContext agentContext, string languageCode, string clientIp)
        {
            return GetCachedAvailability(bookingRequest)
                .Check(CheckRateRestrictions)
                .Map(RegisterBooking)
                .Check(GenerateInvoice)
                .CheckIf(IsDeadlinePassed, ChargeMoney)
                .Map(SendSupplierRequest)
                .Bind(GetAccommodationBookingInfo);


            bool IsDeadlinePassed((Data.Bookings.Booking booking, BookingAvailabilityInfo) bookingInfo)
                => bookingInfo.booking.GetPayDueDate() <= _dateTimeProvider.UtcToday();


            async Task<Result<BookingAvailabilityInfo>> GetCachedAvailability(AccommodationBookingRequest bookingRequest)
                => await _bookingEvaluationStorage.Get(bookingRequest.SearchId,
                    bookingRequest.ResultId,
                    bookingRequest.RoomContractSetId);
            

            Task<Result> CheckRateRestrictions(BookingAvailabilityInfo availabilityInfo) 
                => _rateChecker.Check(bookingRequest, availabilityInfo, PaymentMethods.BankTransfer, agentContext);
            
            
            async Task<(Data.Bookings.Booking, BookingAvailabilityInfo)> RegisterBooking(BookingAvailabilityInfo bookingAvailability)
            {
                var referenceCode = await _bookingRecordManager.Register(bookingRequest, bookingAvailability, PaymentMethods.BankTransfer, agentContext, languageCode);
                var (_, _, booking, _) = await _bookingRecordManager.Get(referenceCode);
                return (booking, bookingAvailability);
            }

            
            async Task<Result> ChargeMoney((Data.Bookings.Booking, BookingAvailabilityInfo) bookingInfo)
            {
                var (booking, _) = bookingInfo;
                return await _accountPaymentService.Charge(booking, agentContext.ToUserInfo());
            }
            
            
            Task<Result> GenerateInvoice((Data.Bookings.Booking, BookingAvailabilityInfo) bookingInfo)
            {
                var (booking, _) = bookingInfo;
                return _documentsService.GenerateInvoice(booking.ReferenceCode);
            }


            async Task<Booking> SendSupplierRequest((Data.Bookings.Booking, BookingAvailabilityInfo) bookingInfo)
            {
                var (booking, availabilityInfo) = bookingInfo;
                return await _requestExecutor.Execute(bookingRequest, 
                    availabilityInfo.AvailabilityId,
                    booking,
                    agentContext,
                    languageCode);
            }


            Task<Result<AccommodationBookingInfo>> GetAccommodationBookingInfo(EdoContracts.Accommodations.Booking details)
                => _bookingInfoService.GetAccommodationBookingInfo(details.ReferenceCode, languageCode);
            
            
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
        
        
        
        private readonly IBookingRecordManager _bookingRecordManager;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingAccountPaymentService _accountPaymentService;
        private readonly IBookingEvaluationStorage _bookingEvaluationStorage;
        private readonly IBookingRateChecker _rateChecker;
        private readonly IBookingDocumentsService _documentsService;
        private readonly IBookingInfoService _bookingInfoService;
        private readonly IBookingRequestExecutor _requestExecutor;
    }
}