using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Models;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments
{
    public class BookingCreditCardPaymentService : IBookingCreditCardPaymentService
    {
        public BookingCreditCardPaymentService(ICreditCardPaymentProcessingService creditCardPaymentProcessingService,
            ILogger<BookingCreditCardPaymentService> logger,
            IDateTimeProvider dateTimeProvider,
            IBookingRecordsManager bookingRecordsManager,
            IBookingPaymentInfoService paymentInfoService)
        {
            _creditCardPaymentProcessingService = creditCardPaymentProcessingService;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _bookingRecordsManager = bookingRecordsManager;
            _paymentInfoService = paymentInfoService;
        }
        

        public async Task<Result<string>> Capture(Booking booking, UserInfo user)
        {
            if (booking.PaymentMethod != PaymentMethods.CreditCard)
            {
                _logger.LogCaptureMoneyForBookingFailure($"Failed to capture money for a booking with reference code: '{booking.ReferenceCode}'. " +
                    $"Error: Invalid payment method: {booking.PaymentMethod}");
                return Result.Failure<string>($"Invalid payment method: {booking.PaymentMethod}");
            }

            _logger.LogCaptureMoneyForBookingSuccess($"Successfully captured money for a booking with reference code: '{booking.ReferenceCode}'");
            return await _creditCardPaymentProcessingService.CaptureMoney(booking.ReferenceCode, user, _paymentInfoService);
        }
        
        
        public async Task<Result> Void(Booking booking, UserInfo user)
        {
            if (booking.PaymentStatus != BookingPaymentStatuses.Authorized)
                return Result.Failure($"Void is only available for payments with '{BookingPaymentStatuses.Authorized}' status");

            return await _creditCardPaymentProcessingService.VoidMoney(booking.ReferenceCode, user, _paymentInfoService);
        }


        public async Task<Result> Refund(Booking booking, UserInfo user)
        {
            if (booking.PaymentStatus != BookingPaymentStatuses.Captured)
                return Result.Failure($"Refund is only available for payments with '{BookingPaymentStatuses.Captured}' status");
            
            var refundableAmount = new MoneyAmount(booking.GetRefundableAmount(_dateTimeProvider.UtcNow()),
                booking.Currency);
                
            return await _creditCardPaymentProcessingService.RefundMoney(booking.ReferenceCode, refundableAmount, user, _paymentInfoService);
        }


        public async Task<Result> PayForAccountBooking(int bookingId, AgentContext agent)
        {
            var (_, _, booking, _) = await _bookingRecordsManager.Get(bookingId, agent.AgentId);
            throw new NotImplementedException();
        }
        
        
        private readonly ICreditCardPaymentProcessingService _creditCardPaymentProcessingService;
        private readonly ILogger<BookingCreditCardPaymentService> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly IBookingPaymentInfoService _paymentInfoService;
    }
}