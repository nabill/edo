using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.PropertyOwners;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution.Flows
{
    public class BankCreditCardBookingFlow : IBankCreditCardBookingFlow
    {
        public BankCreditCardBookingFlow(IBookingRequestStorage requestStorage, IBookingRequestExecutor requestExecutor, 
            IBookingEvaluationStorage evaluationStorage,
            IBookingCreditCardPaymentService creditCardPaymentService, IBookingDocumentsService documentsService,
            IBookingDocumentsMailingService documentsMailingService,
            IBookingInfoService bookingInfoService, IDateTimeProvider dateTimeProvider, IBookingRegistrationService registrationService,
            IBookingConfirmationService bookingConfirmationService, ILogger<BankCreditCardBookingFlow> logger)
        {
            _requestStorage = requestStorage;
            _requestExecutor = requestExecutor;
            _evaluationStorage = evaluationStorage;
            _creditCardPaymentService = creditCardPaymentService;
            _documentsService = documentsService;
            _documentsMailingService = documentsMailingService;
            _bookingInfoService = bookingInfoService;
            _dateTimeProvider = dateTimeProvider;
            _registrationService = registrationService;
            _bookingConfirmationService = bookingConfirmationService;
            _logger = logger;
        }
        
        
        public async Task<Result<string>> Register(AccommodationBookingRequest bookingRequest, AgentContext agentContext, string languageCode)
        {
            Baggage.AddSearchId(bookingRequest.SearchId);
            _logger.LogCreditCardBookingFlowStarted(bookingRequest.HtId);

            var (_, isFailure, booking, error) = await GetCachedAvailability(bookingRequest)
                .Ensure(IsPaymentTypeAllowed, "Payment type is not allowed")
                .Bind(Register)
                .Check(SendEmailToPropertyOwner)
                .Finally(WriteLog);

            if (isFailure)
                return Result.Failure<string>(error);

            return booking.ReferenceCode;

            async Task<Result<BookingAvailabilityInfo>> GetCachedAvailability(AccommodationBookingRequest bookingRequest)
                => await _evaluationStorage.Get(bookingRequest.SearchId, bookingRequest.HtId, bookingRequest.RoomContractSetId);

                
            bool IsPaymentTypeAllowed(BookingAvailabilityInfo availabilityInfo) 
                => availabilityInfo.AvailablePaymentTypes.Contains(PaymentTypes.CreditCard);


            Task<Result<Booking>> Register(BookingAvailabilityInfo bookingAvailability) 
                => _registrationService.Register(bookingRequest, bookingAvailability, PaymentTypes.CreditCard, agentContext, languageCode);


            async Task<Result> SendEmailToPropertyOwner(Booking booking)
                => await _bookingConfirmationService.SendConfirmationEmail(booking);

            
            Result<Booking> WriteLog(Result<Booking> result)
                => LoggerUtils.WriteLogByResult(result,
                    () => _logger.LogBookingRegistrationSuccess(result.Value.ReferenceCode),
                    () => _logger.LogBookingRegistrationFailure(bookingRequest.HtId, bookingRequest.ItineraryNumber, bookingRequest.MainPassengerName, result.Error));
        }
        
        
        public async Task<Result<AccommodationBookingInfo>> Finalize(string referenceCode, AgentContext agentContext, string languageCode)
        {
            return await GetBooking()
                .Check(CheckBookingIsPaid)
                .CheckIf(IsDeadlinePassed, CaptureMoney)
                .Check(GenerateInvoice)
                .Tap(SendReceipt)
                .Bind(SendSupplierRequest)
                .Bind(GetAccommodationBookingInfo)
                .Finally(WriteLog);

            
            Task<Result<Booking>> GetBooking()
                => _bookingInfoService.GetAgentsBooking(referenceCode, agentContext);
            
            
            Result CheckBookingIsPaid(Booking bookingFromPipe)
            {
                if (bookingFromPipe.PaymentStatus != BookingPaymentStatuses.Authorized)
                {
                    _logger.LogBookingFinalizationPaymentFailure(referenceCode);
                    return Result.Failure<Booking>("The booking hasn't been paid");
                }

                return Result.Success();
            }

            
            bool IsDeadlinePassed(Booking booking) 
                => booking.GetPayDueDate() <= _dateTimeProvider.UtcToday();


            async Task<Result> CaptureMoney(Booking booking) 
                => await _creditCardPaymentService.Capture(booking, agentContext.ToApiCaller());
            

            async Task<Result<EdoContracts.Accommodations.Booking>> SendSupplierRequest(Booking booking)
            {
                var (_, isFailure, requestInfo, error) = await _requestStorage.Get(booking.ReferenceCode);
                if(isFailure)
                    return Result.Failure<EdoContracts.Accommodations.Booking>(error);
                
                var (request, availabilityInfo) = requestInfo;
                Baggage.AddSearchId(request.SearchId);
                Baggage.AddBookingReferenceCode(booking.ReferenceCode);

                return await _requestExecutor.Execute(booking, agentContext, languageCode);
            }

            
            Task<Result> GenerateInvoice(Booking booking) 
                => _documentsService.GenerateInvoice(booking);
            
            
            async Task SendReceipt(Booking booking)
            {
                var (_, _, receiptInfo, _) = await _documentsService.GenerateReceipt(booking);
                await _documentsMailingService.SendReceiptToCustomer(receiptInfo, agentContext.Email, 
                    new ApiCaller(agentContext.AgentId.ToString(), ApiCallerTypes.Admin));
            }


            Task<Result<AccommodationBookingInfo>> GetAccommodationBookingInfo(EdoContracts.Accommodations.Booking details)
                => _bookingInfoService.GetAccommodationBookingInfo(details.ReferenceCode, languageCode);
            
            
            Result<AccommodationBookingInfo> WriteLog(Result<AccommodationBookingInfo> result)
                => LoggerUtils.WriteLogByResult(result,
                    () => _logger.LogBookingFinalizationSuccess(result.Value.BookingDetails.ReferenceCode),
                    () => _logger.LogBookingFinalizationFailure(referenceCode, result.Error));
        }
        
        
        private readonly IBookingRequestStorage _requestStorage;
        private readonly IBookingRequestExecutor _requestExecutor;
        private readonly IBookingEvaluationStorage _evaluationStorage;
        private readonly IBookingCreditCardPaymentService _creditCardPaymentService;
        private readonly IBookingDocumentsService _documentsService;
        private readonly IBookingDocumentsMailingService _documentsMailingService;
        private readonly IBookingInfoService _bookingInfoService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingRegistrationService _registrationService;
        private readonly IBookingConfirmationService _bookingConfirmationService;
        private readonly ILogger<BankCreditCardBookingFlow> _logger;
    }
}