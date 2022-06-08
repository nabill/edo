using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution.Flows
{
    public class OfflinePaymentBookingFlow : IOfflinePaymentBookingFlow
    {
        public OfflinePaymentBookingFlow(IDateTimeProvider dateTimeProvider,
            IBookingEvaluationStorage bookingEvaluationStorage,
            IBookingDocumentsService documentsService,
            IBookingInfoService bookingInfoService,
            IBookingRegistrationService registrationService,
            IBookingRequestExecutor requestExecutor, 
            ILogger<OfflinePaymentBookingFlow> logger, 
            IWideAvailabilityStorage availabilityStorage)
        {
            _dateTimeProvider = dateTimeProvider;
            _bookingEvaluationStorage = bookingEvaluationStorage;
            _documentsService = documentsService;
            _bookingInfoService = bookingInfoService;
            _registrationService = registrationService;
            _requestExecutor = requestExecutor;
            _logger = logger;
            _availabilityStorage = availabilityStorage;
        }
        

        public Task<Result<AccommodationBookingInfo>> Book(AccommodationBookingRequest bookingRequest,
            AgentContext agentContext, string languageCode, string clientIp)
        {
            Baggage.AddSearchId(bookingRequest.SearchId);
            _logger.LogBookingByOfflinePaymentStarted(bookingRequest.HtId);

            return GetCachedAvailability(bookingRequest)
                .Ensure(IsPaymentTypeAllowed, "Payment type is not allowed")
                .Ensure(IsDeadlineNotPassed, "Deadline already passed, can not book")
                .Bind(RegisterBooking)
                .Check(GenerateInvoice)
                .Bind(SendSupplierRequest)
                .Bind(GetAccommodationBookingInfo)
                .Tap(ClearCache)
                .Finally(WriteLog);


            bool IsDeadlineNotPassed(BookingAvailabilityInfo bookingAvailability)
            {
                var deadlineDate = bookingAvailability.RoomContractSet.Deadline.Date;
                var dueDate = deadlineDate == null || deadlineDate == DateTime.MinValue ? bookingAvailability.CheckInDate : deadlineDate.Value;
                return _dateTimeProvider.UtcToday() < dueDate - BookingPaymentTypesHelper.OfflinePaymentAdditionalDays;
            }


            async Task<Result<BookingAvailabilityInfo>> GetCachedAvailability(AccommodationBookingRequest bookingRequest)
                => await _bookingEvaluationStorage.Get(bookingRequest.SearchId,
                    bookingRequest.HtId,
                    bookingRequest.RoomContractSetId);
            

            bool IsPaymentTypeAllowed(BookingAvailabilityInfo availabilityInfo) 
                => availabilityInfo.AvailablePaymentTypes.Contains(PaymentTypes.Offline);


            Task<Result<Data.Bookings.Booking>> RegisterBooking(BookingAvailabilityInfo bookingAvailability) 
                => _registrationService.Register(bookingRequest, bookingAvailability, PaymentTypes.Offline, agentContext, languageCode);


            Task<Result> GenerateInvoice(Data.Bookings.Booking booking) 
                => _documentsService.GenerateInvoice(booking);


            async Task<Result<Booking>> SendSupplierRequest(Data.Bookings.Booking booking)
            {
                return await _requestExecutor.Execute(booking,
                    agentContext,
                    languageCode);
            }


            Task<Result<AccommodationBookingInfo>> GetAccommodationBookingInfo(EdoContracts.Accommodations.Booking details)
                => _bookingInfoService.GetAccommodationBookingInfo(details.ReferenceCode, languageCode);
            
            
            Task ClearCache(AccommodationBookingInfo accommodationBooking) 
                => _availabilityStorage.Clear(accommodationBooking.Supplier, bookingRequest.SearchId);
            
            
            Result<AccommodationBookingInfo> WriteLog(Result<AccommodationBookingInfo> result)
                => LoggerUtils.WriteLogByResult(result,
                    () => _logger.LogBookingByAccountSuccess(result.Value.BookingDetails.ReferenceCode),
                    () => _logger.LogBookingByOfflinePaymentFailure(bookingRequest.HtId, result.Error));
        }

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingEvaluationStorage _bookingEvaluationStorage;
        private readonly IBookingDocumentsService _documentsService;
        private readonly IBookingInfoService _bookingInfoService;
        private readonly IBookingRegistrationService _registrationService;
        private readonly IBookingRequestExecutor _requestExecutor;
        private readonly ILogger<OfflinePaymentBookingFlow> _logger;
        private readonly IWideAvailabilityStorage _availabilityStorage;
    }
}