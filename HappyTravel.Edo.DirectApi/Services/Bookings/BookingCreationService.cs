using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.DirectApi.Models.Booking;
using AvailabilityRequest = HappyTravel.Edo.Api.Models.Availabilities.AvailabilityRequest;

namespace HappyTravel.Edo.DirectApi.Services.Bookings
{
    public class BookingCreationService
    {
        public BookingCreationService(ClientReferenceCodeValidationService validationService, 
            IBookingRegistrationService bookingRegistrationService, IBookingEvaluationStorage bookingEvaluationStorage,
            BookingInfoService bookingInfoService, IBookingDocumentsService documentsService, IDateTimeProvider dateTimeProvider, 
            IBookingAccountPaymentService accountPaymentService, IBookingRequestExecutor requestExecutor, IAvailabilityRequestStorage availabilityRequestStorage)
        {
            _validationService = validationService;
            _bookingRegistrationService = bookingRegistrationService;
            _bookingEvaluationStorage = bookingEvaluationStorage;
            _bookingInfoService = bookingInfoService;
            _documentsService = documentsService;
            _dateTimeProvider = dateTimeProvider;
            _accountPaymentService = accountPaymentService;
            _requestExecutor = requestExecutor;
            _availabilityRequestStorage = availabilityRequestStorage;
        }


        public async Task<Result<Booking>> Register(AccommodationBookingRequest request, AgentContext agent, string languageCode)
        {
            return await _validationService.Validate(request.ClientReferenceCode, agent)
                .Bind(GetCachedAvailability)
                .Bind(RegisterBooking);
            
            
            async Task<Result<(AvailabilityRequest, BookingAvailabilityInfo)>> GetCachedAvailability()
            {
                var availabilityInfo = await _bookingEvaluationStorage.Get(request.SearchId,
                    request.AccommodationId,
                    request.RoomContractSetId);

                if (availabilityInfo.IsFailure)
                    return Result.Failure<(AvailabilityRequest, BookingAvailabilityInfo)>(availabilityInfo.Error);

                var availabilityRequest = await _availabilityRequestStorage.Get(request.SearchId);
                if (availabilityRequest.IsFailure)
                    return Result.Failure<(AvailabilityRequest, BookingAvailabilityInfo)>(availabilityRequest.Error);

                return (availabilityRequest.Value, availabilityInfo.Value);
            }


            async Task<Result<Booking>> RegisterBooking((AvailabilityRequest AvailabilityRequest, BookingAvailabilityInfo BookingAvailabilityInfo) data)
            {
                var booking =  await _bookingRegistrationService.Register(bookingRequest: request.ToEdoModel(), 
                    availabilityInfo: data.BookingAvailabilityInfo, 
                    paymentMethod: PaymentTypes.VirtualAccount, 
                    agentContext: agent, 
                    languageCode: languageCode,
                    nationality: data.AvailabilityRequest.Nationality,
                    residency: data.AvailabilityRequest.Residency);

                return booking.FromEdoModels();
            }
        }


        public async Task<Result<Booking>> Finalize(string clientReferenceCode, AgentContext agent, string languageCode)
        {
            return await _bookingInfoService.Get(clientReferenceCode, agent)
                .Check(GenerateInvoice)
                .CheckIf(IsDeadlinePassed, ChargeMoney)
                .Bind(SendSupplierRequest);
            
            
            Task<Result> GenerateInvoice(Data.Bookings.Booking booking) 
                => _documentsService.GenerateInvoice(booking);
            
            
            bool IsDeadlinePassed(Data.Bookings.Booking booking)
                => booking.GetPayDueDate() <= _dateTimeProvider.UtcToday();
            
            
            async Task<Result> ChargeMoney(Data.Bookings.Booking booking) 
                => await _accountPaymentService.Charge(booking, agent.ToApiCaller());
            
            
            async Task<Result<Booking>> SendSupplierRequest(Data.Bookings.Booking booking)
            { 
                var result =  await _requestExecutor.Execute(booking, agent, languageCode);
                if (result.IsFailure)
                    return Result.Failure<Booking>(result.Error);

                var refreshedBooking = await _bookingInfoService.Get(clientReferenceCode, agent);
                return refreshedBooking.Value.FromEdoModels();
            }
        }

        
        private readonly ClientReferenceCodeValidationService _validationService;
        private readonly IBookingRegistrationService _bookingRegistrationService;
        private readonly IBookingEvaluationStorage _bookingEvaluationStorage;
        private readonly BookingInfoService _bookingInfoService;
        private readonly IBookingDocumentsService _documentsService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingAccountPaymentService _accountPaymentService;
        private readonly IBookingRequestExecutor _requestExecutor;
        private readonly IAvailabilityRequestStorage _availabilityRequestStorage;
    }
}