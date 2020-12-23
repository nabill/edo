using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments
{
    public class BookingCreditCardPaymentService : IBookingCreditCardPaymentService
    {
        public BookingCreditCardPaymentService(ICreditCardPaymentProcessingService creditCardPaymentProcessingService,
            ILogger<BookingCreditCardPaymentService> logger,
            IBookingPaymentInfoService paymentInfoService)
        {
            _creditCardPaymentProcessingService = creditCardPaymentProcessingService;
            _logger = logger;
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
        
        
        private readonly ICreditCardPaymentProcessingService _creditCardPaymentProcessingService;
        private readonly ILogger<BookingCreditCardPaymentService> _logger;
        private readonly IBookingPaymentInfoService _paymentInfoService;
    }
}