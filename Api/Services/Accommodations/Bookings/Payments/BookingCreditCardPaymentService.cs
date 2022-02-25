using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments
{
    public class BookingCreditCardPaymentService : IBookingCreditCardPaymentService
    {
        public BookingCreditCardPaymentService(ICreditCardPaymentProcessingService creditCardPaymentProcessingService,
            ILogger<BookingCreditCardPaymentService> logger,
            IDateTimeProvider dateTimeProvider, BookingDocumentsMailingService documentsMailingService,
            IBookingInfoService bookingInfoService, IBookingDocumentsService documentsService, 
            IBookingPaymentCallbackService paymentCallbackService)
        {
            _creditCardPaymentProcessingService = creditCardPaymentProcessingService;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _documentsMailingService = documentsMailingService;
            _bookingInfoService = bookingInfoService;
            _documentsService = documentsService;
            _paymentCallbackService = paymentCallbackService;
        }
        

        public async Task<Result<string>> Capture(Booking booking, ApiCaller apiCaller)
        {
            if (booking.PaymentType != PaymentTypes.CreditCard)
            {
                _logger.LogCaptureMoneyForBookingFailure(booking.ReferenceCode, booking.PaymentType);
                return Result.Failure<string>($"Invalid payment method: {booking.PaymentType}");
            }

            var result =  await _creditCardPaymentProcessingService.CaptureMoney(booking.ReferenceCode, apiCaller, _paymentCallbackService);
            if (result.IsSuccess)
                _logger.LogCaptureMoneyForBookingSuccess(booking.ReferenceCode);

            return result;
        }
        
        
        public async Task<Result> Void(Booking booking, ApiCaller apiCaller)
        {
            if (booking.PaymentStatus != BookingPaymentStatuses.Authorized)
                return Result.Failure($"Void is only available for payments with '{BookingPaymentStatuses.Authorized}' status");

            return await _creditCardPaymentProcessingService.VoidMoney(booking.ReferenceCode, apiCaller, _paymentCallbackService);
        }


        public async Task<Result> Refund(Booking booking, DateTime operationDate, ApiCaller apiCaller)
        {
            if (booking.PaymentStatus != BookingPaymentStatuses.Captured)
                return Result.Failure($"Refund is only available for payments with '{BookingPaymentStatuses.Captured}' status");
            
            return await _creditCardPaymentProcessingService.RefundMoney(booking.ReferenceCode, apiCaller, operationDate, _paymentCallbackService);
        }


        public async Task<Result> PayForAccountBooking(string referenceCode, AgentContext agent)
        {
            return await GetBooking(referenceCode, agent)
                .Ensure(IsBookingPaid, "Failed to pay for booking")
                .CheckIf(IsDeadlinePassed, CaptureMoney)
                .Tap(SendReceipt);


            Task<Result<Booking>> GetBooking(string code, AgentContext agentContext) 
                => _bookingInfoService.GetAgentsBooking(code, agentContext);


            bool IsBookingPaid(Booking booking) 
                => booking.PaymentStatus == BookingPaymentStatuses.Authorized;
            
            
            bool IsDeadlinePassed(Booking booking) 
                => booking.GetPayDueDate() <= _dateTimeProvider.UtcToday();
            

            async Task<Result> CaptureMoney(Booking booking) 
                => await Capture(booking, agent.ToApiCaller());
            
            
            async Task SendReceipt(Booking booking)
            {
                var (_, _, receiptInfo, _) = await _documentsService.GenerateReceipt(booking);

                await _documentsMailingService.SendReceiptToCustomer(receiptInfo, agent.Email, new ApiCaller(agent.Email, ApiCallerTypes.Agent));
            }
        }
        
        
        private readonly ICreditCardPaymentProcessingService _creditCardPaymentProcessingService;
        private readonly ILogger<BookingCreditCardPaymentService> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly BookingDocumentsMailingService _documentsMailingService;
        private readonly IBookingInfoService _bookingInfoService;
        private readonly IBookingDocumentsService _documentsService;
        private readonly IBookingPaymentCallbackService _paymentCallbackService;
    }
}