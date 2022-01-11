using System;
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
using AvailabilityRequest = HappyTravel.Edo.Api.Models.Availabilities.AvailabilityRequest;

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
            ILogger<OfflinePaymentBookingFlow> logger, IAvailabilityRequestStorage availabilityRequestStorage)
        {
            _dateTimeProvider = dateTimeProvider;
            _bookingEvaluationStorage = bookingEvaluationStorage;
            _documentsService = documentsService;
            _bookingInfoService = bookingInfoService;
            _registrationService = registrationService;
            _requestExecutor = requestExecutor;
            _logger = logger;
            _availabilityRequestStorage = availabilityRequestStorage;
        }
        

        public Task<Result<AccommodationBookingInfo>> Book(AccommodationBookingRequest bookingRequest,
            AgentContext agentContext, string languageCode, string clientIp)
        {
            Baggage.AddSearchId(bookingRequest.SearchId);
            _logger.LogBookingByOfflinePaymentStarted(bookingRequest.HtId);

            return GetCachedAvailability(bookingRequest)
                .Ensure(IsPaymentTypeAllowed, "Payment type is not allowed")
                .Ensure(IsDeadlineNotPassed, "Deadline already passed, can not book")
                .Map(RegisterBooking)
                .Check(GenerateInvoice)
                .Bind(SendSupplierRequest)
                .Bind(GetAccommodationBookingInfo)
                .Finally(WriteLog);


            bool IsDeadlineNotPassed((AvailabilityRequest AvailabilityRequest, BookingAvailabilityInfo BookingAvailabilityInfo) data)
            {
                var deadlineDate = data.BookingAvailabilityInfo.RoomContractSet.Deadline.Date;
                var dueDate = deadlineDate == null || deadlineDate == DateTime.MinValue 
                    ? data.BookingAvailabilityInfo.CheckInDate 
                    : deadlineDate.Value;
                
                return _dateTimeProvider.UtcToday() < dueDate - BookingPaymentTypesHelper.OfflinePaymentAdditionalDays;
            }


            async Task<Result<(AvailabilityRequest, BookingAvailabilityInfo)>> GetCachedAvailability(AccommodationBookingRequest bookingRequest)
            {
                var availabilityInfo = await _bookingEvaluationStorage.Get(bookingRequest.SearchId,
                    bookingRequest.HtId,
                    bookingRequest.RoomContractSetId);

                if (availabilityInfo.IsFailure)
                    return Result.Failure<(AvailabilityRequest, BookingAvailabilityInfo)>(availabilityInfo.Error);

                var availabilityRequest = await _availabilityRequestStorage.Get(bookingRequest.SearchId);
                if (availabilityRequest.IsFailure)
                    return Result.Failure<(AvailabilityRequest, BookingAvailabilityInfo)>(availabilityRequest.Error);

                return (availabilityRequest.Value, availabilityInfo.Value);
            }


            bool IsPaymentTypeAllowed((AvailabilityRequest AvailabilityRequest, BookingAvailabilityInfo BookingAvailabilityInfo) data) 
                => data.BookingAvailabilityInfo.AvailablePaymentTypes.Contains(PaymentTypes.Offline);


            Task<Data.Bookings.Booking> RegisterBooking((AvailabilityRequest AvailabilityRequest, BookingAvailabilityInfo BookingAvailabilityInfo) data) 
                => _registrationService.Register(bookingRequest: bookingRequest, 
                    availabilityInfo: data.BookingAvailabilityInfo, 
                    paymentMethod: PaymentTypes.Offline, 
                    agentContext: agentContext, 
                    languageCode: languageCode,
                    nationality: data.AvailabilityRequest.Nationality,
                    residency: data.AvailabilityRequest.Residency);


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
        private readonly IAvailabilityRequestStorage _availabilityRequestStorage;
    }
}