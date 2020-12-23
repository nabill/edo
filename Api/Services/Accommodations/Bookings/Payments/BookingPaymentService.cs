using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments
{
    public class BookingPaymentService : IBookingPaymentService
    {
        public BookingPaymentService(IAccountPaymentService accountPaymentService,
            ICreditCardPaymentProcessingService creditCardPaymentProcessingService,
            IBookingPaymentInfoService paymentInfoService)
        {
            _accountPaymentService = accountPaymentService;
            _creditCardPaymentProcessingService = creditCardPaymentProcessingService;
            _paymentInfoService = paymentInfoService;
        }



        public Task<Result> VoidOrRefund(Booking booking, UserInfo user)
        {
            // TODO: Add logging

            switch (booking.PaymentMethod)
            {
                case PaymentMethods.BankTransfer:
                    return RefundBankTransfer();
                case PaymentMethods.CreditCard:
                    return VoidOrRefundCard();
                default: 
                    return Task.FromResult(Result.Failure($"Could not void money for the booking with a payment method '{booking.PaymentMethod}'"));
            }


            Task<Result> VoidOrRefundCard()
            {
                if (booking.PaymentStatus == BookingPaymentStatuses.Captured)
                    return _creditCardPaymentProcessingService.RefundMoney(booking.ReferenceCode, user, _paymentInfoService);

                if (booking.PaymentStatus != BookingPaymentStatuses.Authorized)
                    return Task.FromResult(Result.Success());

                return _creditCardPaymentProcessingService.VoidMoney(booking.ReferenceCode, user, _paymentInfoService);
            }


            Task<Result> RefundBankTransfer()
            {
                if (booking.PaymentStatus != BookingPaymentStatuses.Captured)
                    return Task.FromResult(Result.Success());

                return _accountPaymentService.Refund(booking, user);
            }
        }
        

        private readonly IAccountPaymentService _accountPaymentService;
        private readonly ICreditCardPaymentProcessingService _creditCardPaymentProcessingService;
        private readonly IBookingPaymentInfoService _paymentInfoService;
    }
}