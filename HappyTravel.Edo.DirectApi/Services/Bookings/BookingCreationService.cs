using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.DirectApi.Models;
using HappyTravel.Edo.DirectApi.Models.Booking;

namespace HappyTravel.Edo.DirectApi.Services.Bookings
{
    public class BookingCreationService
    {
        public BookingCreationService(ClientReferenceCodeValidationService validationService, 
            IBookingRegistrationService bookingRegistrationService, IBookingEvaluationStorage bookingEvaluationStorage,
            BookingInfoService bookingInfoService, IBookingDocumentsService documentsService, IDateTimeProvider dateTimeProvider, 
            IBookingAccountPaymentService accountPaymentService, IBookingRequestExecutor requestExecutor)
        {
            _validationService = validationService;
            _bookingRegistrationService = bookingRegistrationService;
            _bookingEvaluationStorage = bookingEvaluationStorage;
            _bookingInfoService = bookingInfoService;
            _documentsService = documentsService;
            _dateTimeProvider = dateTimeProvider;
            _accountPaymentService = accountPaymentService;
            _requestExecutor = requestExecutor;
        }


        public async Task<Result<Booking>> Register(AccommodationBookingRequest request, AgentContext agent, string languageCode)
        {
            return await _validationService.Validate(request.ClientReferenceCode, agent)
                .Bind(GetCachedAvailability)
                .Bind(RegisterBooking);
            
            
            async Task<Result<BookingAvailabilityInfo>> GetCachedAvailability()
                => await _bookingEvaluationStorage.Get(request.SearchId,
                    request.AccommodationId,
                    request.RoomContractSetId);


            async Task<Result<Booking>> RegisterBooking(BookingAvailabilityInfo availabilityInfo)
            {
                var booking =  await _bookingRegistrationService.Register(bookingRequest: request.ToEdoModel(), 
                    availabilityInfo: availabilityInfo, 
                    paymentMethod: PaymentTypes.VirtualAccount, 
                    agentContext: agent, 
                    languageCode: languageCode);

                return booking.IsSuccess
                    ? booking.Value.FromEdoModels()
                    : Result.Failure<Booking>(booking.Error);
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
    }
}