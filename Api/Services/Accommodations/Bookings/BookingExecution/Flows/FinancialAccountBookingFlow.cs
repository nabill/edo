using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution.Flows
{
    public class FinancialAccountBookingFlow : IFinancialAccountBookingFlow
    {
        public FinancialAccountBookingFlow(IDateTimeProvider dateTimeProvider,
            IBookingAccountPaymentService accountPaymentService,
            IBookingEvaluationStorage bookingEvaluationStorage,
            IBookingDocumentsService documentsService,
            IBookingInfoService bookingInfoService,
            IBookingRegistrationService registrationService,
            IBookingRequestExecutor requestExecutor,
            IBookingRecordManager recordManager,
            ILogger<FinancialAccountBookingFlow> logger, 
            IWideAvailabilityStorage availabilityStorage)
        {
            _dateTimeProvider = dateTimeProvider;
            _accountPaymentService = accountPaymentService;
            _bookingEvaluationStorage = bookingEvaluationStorage;
            _documentsService = documentsService;
            _bookingInfoService = bookingInfoService;
            _registrationService = registrationService;
            _requestExecutor = requestExecutor;
            _recordManager = recordManager;
            _logger = logger;
            _availabilityStorage = availabilityStorage;
        }


        public Task<Result<AccommodationBookingInfo>> BookByAccount(AccommodationBookingRequest bookingRequest,
            AgentContext agentContext, string languageCode, string clientIp)
        {
            Baggage.AddSearchId(bookingRequest.SearchId);
            _logger.LogBookingByAccountStarted(bookingRequest.HtId);

            return GetCachedAvailability(bookingRequest)
                .Ensure(IsPaymentTypeAllowed, "Payment type is not allowed")
                .Bind(RegisterBooking)
                .Check(GenerateInvoice)
                .CheckIf(IsDeadlinePassed, ChargeMoney)
                .Bind(SendSupplierRequest)
                .Bind(GetAccommodationBookingInfo)
                .Tap(ClearCache)
                .Finally(WriteLog);


            bool IsDeadlinePassed(Data.Bookings.Booking booking)
                => booking.GetPayDueDate() <= _dateTimeProvider.UtcToday();


            async Task<Result<BookingAvailabilityInfo>> GetCachedAvailability(AccommodationBookingRequest bookingRequest)
                => await _bookingEvaluationStorage.Get(bookingRequest.SearchId,
                    bookingRequest.HtId,
                    bookingRequest.RoomContractSetId);
            

            bool IsPaymentTypeAllowed(BookingAvailabilityInfo availabilityInfo) 
                => availabilityInfo.AvailablePaymentTypes.Contains(PaymentTypes.VirtualAccount);


            Task<Result<Data.Bookings.Booking>> RegisterBooking(BookingAvailabilityInfo bookingAvailability) 
                => _registrationService.Register(bookingRequest, bookingAvailability, PaymentTypes.VirtualAccount, agentContext, languageCode);


            async Task<Result> ChargeMoney(Data.Bookings.Booking booking) 
                => await _accountPaymentService.Charge(booking, agentContext.ToApiCaller());


            Task<Result> GenerateInvoice(Data.Bookings.Booking booking) 
                => _documentsService.GenerateInvoice(booking);


            async Task<Result<Booking>> SendSupplierRequest(Data.Bookings.Booking booking)
            {
                var refreshedBooking = await _recordManager.Get(booking.ReferenceCode);
                return await _requestExecutor.Execute(refreshedBooking.Value, agentContext, languageCode);
            }


            Task<Result<AccommodationBookingInfo>> GetAccommodationBookingInfo(EdoContracts.Accommodations.Booking details)
                => _bookingInfoService.GetAccommodationBookingInfo(details.ReferenceCode, languageCode);


            Task ClearCache(AccommodationBookingInfo accommodationBooking) 
                => _availabilityStorage.Clear(accommodationBooking.Supplier, bookingRequest.SearchId);


            Result<AccommodationBookingInfo> WriteLog(Result<AccommodationBookingInfo> result)
                => LoggerUtils.WriteLogByResult(result,
                    () => _logger.LogBookingByAccountSuccess(result.Value.BookingDetails.ReferenceCode),
                    () => _logger.LogBookingByAccountFailure(bookingRequest.HtId, result.Error));
        }


        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingAccountPaymentService _accountPaymentService;
        private readonly IBookingEvaluationStorage _bookingEvaluationStorage;
        private readonly IBookingDocumentsService _documentsService;
        private readonly IBookingInfoService _bookingInfoService;
        private readonly IBookingRegistrationService _registrationService;
        private readonly IBookingRecordManager _recordManager;
        private readonly IBookingRequestExecutor _requestExecutor;
        private readonly ILogger<FinancialAccountBookingFlow> _logger;
        private readonly IWideAvailabilityStorage _availabilityStorage;
    }
}