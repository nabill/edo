using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.Edo.Api.Services.Payments.Offline;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Data.Management;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments
{
    public class BookingPaymentService : IBookingPaymentService
    {
        public BookingPaymentService(EdoContext context,
            IAccountPaymentService accountPaymentService,
            ICreditCardPaymentProcessingService creditCardPaymentProcessingService,
            IBookingRecordsManager recordsManager,
            IBookingDocumentsService documentsService,
            IPaymentNotificationService notificationService,
            IBookingPaymentInfoService paymentInfoService,
            ILogger<BookingPaymentService> logger,
            IOfflinePaymentAuditService offlinePaymentAuditService)
        {
            _context = context;
            _accountPaymentService = accountPaymentService;
            _creditCardPaymentProcessingService = creditCardPaymentProcessingService;
            _recordsManager = recordsManager;
            _documentsService = documentsService;
            _notificationService = notificationService;
            _paymentInfoService = paymentInfoService;
            _logger = logger;
            _offlinePaymentAuditService = offlinePaymentAuditService;
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


        public async Task<Result> CompleteOffline(int bookingId, Administrator administratorContext)
        {
            // TODO: Add admin actions audit log NIJO-659
            return await GetBooking()
                .Bind(CheckBookingCanBeCompleted)
                .Tap(Complete)
                .Tap(WriteAuditLog);


            async Task<Result<Booking>> GetBooking()
            {
                var (_, isFailure, booking, _) = await _recordsManager.Get(bookingId);
                return isFailure
                    ? Result.Failure<Booking>($"Could not find booking with id {bookingId}")
                    : Result.Success(booking);
            }


            Result<Booking> CheckBookingCanBeCompleted(Booking booking)
                => booking.PaymentStatus == BookingPaymentStatuses.NotPaid
                    ? Result.Success(booking)
                    : Result.Failure<Booking>($"Could not complete booking. Invalid payment status: {booking.PaymentStatus}");


            Task Complete(Booking booking)
            {
                booking.PaymentMethod = PaymentMethods.Offline;
                return ChangeBookingPaymentStatusToCaptured(booking);
            }


            Task WriteAuditLog(Booking booking) => _offlinePaymentAuditService.Write(administratorContext.ToUserInfo(), booking.ReferenceCode);
        }


        private Task ChangeBookingPaymentStatusToCaptured(Booking booking)
        {
            booking.PaymentStatus = BookingPaymentStatuses.Captured;
            _context.Bookings.Update(booking);
            return _context.SaveChangesAsync();
        }
        

        private readonly EdoContext _context;
        private readonly IAccountPaymentService _accountPaymentService;
        private readonly ICreditCardPaymentProcessingService _creditCardPaymentProcessingService;
        private readonly IBookingRecordsManager _recordsManager;
        private readonly IBookingDocumentsService _documentsService;
        private readonly IPaymentNotificationService _notificationService;
        private readonly IBookingPaymentInfoService _paymentInfoService;
        private readonly ILogger<BookingPaymentService> _logger;
        private readonly IOfflinePaymentAuditService _offlinePaymentAuditService;
    }
}