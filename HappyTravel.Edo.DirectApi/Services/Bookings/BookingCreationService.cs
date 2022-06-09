using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.DirectApi.Infrastructure;
using HappyTravel.Edo.DirectApi.Models.Booking;

namespace HappyTravel.Edo.DirectApi.Services.Bookings
{
    public class BookingCreationService
    {
        public BookingCreationService(ClientReferenceCodeValidationService validationService, 
            IBookingRegistrationService bookingRegistrationService, IBookingEvaluationStorage bookingEvaluationStorage,
            BookingInfoService bookingInfoService, IBookingDocumentsService documentsService, IDateTimeProvider dateTimeProvider, 
            IBookingAccountPaymentService accountPaymentService, IBookingRequestExecutor requestExecutor, IEvaluationTokenStorage tokenStorage)
        {
            _validationService = validationService;
            _bookingRegistrationService = bookingRegistrationService;
            _bookingEvaluationStorage = bookingEvaluationStorage;
            _bookingInfoService = bookingInfoService;
            _documentsService = documentsService;
            _dateTimeProvider = dateTimeProvider;
            _accountPaymentService = accountPaymentService;
            _requestExecutor = requestExecutor;
            _tokenStorage = tokenStorage;
        }


        public async Task<Result<Booking>> Register(AccommodationBookingRequest request)
        {
            return await _validationService.Validate(request.ClientReferenceCode)
                .Bind(CheckEvaluationToken)
                .Bind(GetCachedAvailability)
                .Bind(RegisterBooking);


            async Task<Result> CheckEvaluationToken()
            {
                var isTokenExists = await _tokenStorage.IsExists(request.EvaluationToken, request.RoomContractSetId);
                return isTokenExists
                    ? Result.Success()
                    : Result.Failure("Evaluation token is not exists");
            }
            
            async Task<Result<BookingAvailabilityInfo>> GetCachedAvailability()
                => await _bookingEvaluationStorage.Get(request.SearchId,
                    request.AccommodationId,
                    request.RoomContractSetId);


            async Task<Result<Booking>> RegisterBooking(BookingAvailabilityInfo availabilityInfo)
            {
                var booking =  await _bookingRegistrationService.Register(bookingRequest: request.ToEdoModel(), 
                    availabilityInfo: availabilityInfo, 
                    paymentMethod: PaymentTypes.VirtualAccount,
                    languageCode: Constants.LanguageCode);

                return booking.IsSuccess
                    ? booking.Value.FromEdoModels()
                    : Result.Failure<Booking>(booking.Error);
            }
        }


        public async Task<Result<Booking>> Finalize(string clientReferenceCode)
        {
            return await _bookingInfoService.Get(clientReferenceCode)
                .Check(GenerateInvoice)
                .CheckIf(IsDeadlinePassed, ChargeMoney)
                .Bind(SendSupplierRequest);
            
            
            Task<Result> GenerateInvoice(Data.Bookings.Booking booking) 
                => _documentsService.GenerateInvoice(booking);
            
            
            bool IsDeadlinePassed(Data.Bookings.Booking booking)
                => booking.GetPayDueDate() <= _dateTimeProvider.UtcToday();
            
            
            async Task<Result> ChargeMoney(Data.Bookings.Booking booking) 
                => await _accountPaymentService.Charge(booking);
            
            
            async Task<Result<Booking>> SendSupplierRequest(Data.Bookings.Booking booking)
            { 
                var result =  await _requestExecutor.Execute(booking, Constants.LanguageCode);
                if (result.IsFailure)
                    return Result.Failure<Booking>(result.Error);

                var refreshedBooking = await _bookingInfoService.Get(clientReferenceCode);
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
        private readonly IEvaluationTokenStorage _tokenStorage;
    }
}