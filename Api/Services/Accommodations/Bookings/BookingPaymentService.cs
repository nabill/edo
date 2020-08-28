using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.Data.Management;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingPaymentService : IBookingPaymentService
    {
        public BookingPaymentService(EdoContext context,
            IAccountPaymentService accountPaymentService,
            ICreditCardPaymentProcessingService creditCardPaymentProcessingService,
            IBookingRecordsManager recordsManager,
            ILogger<BookingPaymentService> logger)
        {
            _context = context;
            _accountPaymentService = accountPaymentService;
            _creditCardPaymentProcessingService = creditCardPaymentProcessingService;
            _recordsManager = recordsManager;
            _logger = logger;
        }


        public async Task<Result<string>> Capture(Booking booking, UserInfo user)
        {
            Result<string> result;
            switch (booking.PaymentMethod)
            {
                case PaymentMethods.BankTransfer:
                    result = await _accountPaymentService.Capture(booking, user);
                    break;
                case PaymentMethods.CreditCard:
                    result = await _creditCardPaymentProcessingService.CaptureMoney(booking.ReferenceCode, user, this);
                    break;
                default:
                    result = Result.Failure<string>($"Invalid payment method: {booking.PaymentMethod}");
                    break;
            }

            if (result.IsSuccess)
                _logger.LogCaptureMoneyForBookingSuccess($"Successfully captured money for a booking with reference code: '{booking.ReferenceCode}'");
            else
                _logger.LogCaptureMoneyForBookingFailure($"Failed to capture money for a booking with reference code: '{booking.ReferenceCode}'. " +
                    $"Error: {result.Error}");

            return result;
        }


        public Task<Result> VoidOrRefund(Booking booking, UserInfo user)
        {
            // TODO: Add logging

            switch (booking.PaymentMethod)
            {
                case PaymentMethods.BankTransfer:
                    return RefundBankTransfer();
                case PaymentMethods.CreditCard:
                    return VoidCard();
                default: 
                    return Task.FromResult(Result.Failure($"Could not void money for the booking with a payment method '{booking.PaymentMethod}'"));
            }


            Task<Result> VoidCard()
            {
                if (booking.PaymentStatus != BookingPaymentStatuses.Authorized)
                    return Task.FromResult(Result.Ok());

                return _creditCardPaymentProcessingService.VoidMoney(booking.ReferenceCode, user, this);
            }


            Task<Result> RefundBankTransfer()
            {
                if (booking.PaymentStatus != BookingPaymentStatuses.Captured)
                    return Task.FromResult(Result.Ok());

                return _accountPaymentService.Refund(booking, user);
            }
        }


        public async Task<Result> CompleteOffline(int bookingId, Administrator administratorContext)
        {
            // TODO: Add admin actions audit log NIJO-659
            return await GetBooking()
                .Bind(CheckBookingCanBeCompleted)
                .Tap(Complete);


            async Task<Result<Booking>> GetBooking()
            {
                var (_, isFailure, booking, _) = await _recordsManager.Get(bookingId);
                return isFailure
                    ? Result.Failure<Booking>($"Could not find booking with id {bookingId}")
                    : Result.Ok(booking);
            }


            Result<Booking> CheckBookingCanBeCompleted(Booking booking)
                => booking.PaymentStatus == BookingPaymentStatuses.NotPaid
                    ? Result.Ok(booking)
                    : Result.Failure<Booking>($"Could not complete booking. Invalid payment status: {booking.PaymentStatus}");


            Task Complete(Booking booking)
            {
                booking.PaymentMethod = PaymentMethods.Offline;
                return ChangeBookingPaymentStatusToCaptured(booking);
            }
        }


        private Task ChangeBookingPaymentStatusToCaptured(Booking booking)
        {
            booking.PaymentStatus = BookingPaymentStatuses.Captured;
            _context.Bookings.Update(booking);
            return _context.SaveChangesAsync();
        }
        
        
        
        public async Task<Result<MoneyAmount>> GetServicePrice(string referenceCode)
        {
            var booking = await _context.Bookings.SingleOrDefaultAsync(b => b.ReferenceCode == referenceCode);
            if(booking == default)
                return Result.Failure<MoneyAmount>("Could not find booking");

            return Result.Ok(new MoneyAmount(booking.TotalPrice, booking.Currency));
        }


        public async Task<Result> ProcessPaymentChanges(Payment payment)
        {
            var booking = await _context.Bookings.SingleOrDefaultAsync(b => b.ReferenceCode == payment.ReferenceCode);
            if (booking == default)
            {
                _logger.LogProcessPaymentChangesForBookingFailure("Failed to process payment changes, " +
                    $"could not find the corresponding booking. Payment status: {payment.Status}. Payment: '{payment.ReferenceCode}'");

                return Result.Failure($"Could not find booking for payment '{payment.ReferenceCode}'");
            }

            var oldPaymentStatus = booking.PaymentStatus;

            switch (payment.Status)
            {
                case PaymentStatuses.Authorized:
                    booking.PaymentStatus = BookingPaymentStatuses.Authorized;
                    break;
                case PaymentStatuses.Captured:
                    booking.PaymentStatus = BookingPaymentStatuses.Captured;
                    break;
                case PaymentStatuses.Voided:
                    booking.PaymentStatus = BookingPaymentStatuses.Voided;
                    break;
                case PaymentStatuses.Refunded:
                    booking.PaymentStatus = BookingPaymentStatuses.Refunded;
                    break;
                default: 
                    _logger.LogProcessPaymentChangesForBookingSkip("Skipped booking status update while processing payment changes. " +
                        $"Payment status: {payment.Status}. Payment: '{payment.ReferenceCode}'. Booking reference code: '{booking.ReferenceCode}'");

                    return Result.Ok();
            }

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            
            _context.Entry(booking).State = EntityState.Detached;

            _logger.LogProcessPaymentChangesForBookingSuccess($"Successfully processes payment changes. Old payment status: {oldPaymentStatus}. " +
                $"New payment status: {payment.Status}. Payment: '{payment.ReferenceCode}'. Booking reference code: '{booking.ReferenceCode}'");

            return Result.Ok();
        }


        public async Task<Result<(int AgentId, int AgencyId)>> GetServiceBuyer(string referenceCode)
        {
            var (_, isFailure, booking, error) = await _recordsManager.Get(referenceCode);
            if (isFailure)
                return Result.Failure<(int, int)>(error);

            return (booking.AgentId, booking.AgencyId);
        }


        private readonly EdoContext _context;
        private readonly IAccountPaymentService _accountPaymentService;
        private readonly ICreditCardPaymentProcessingService _creditCardPaymentProcessingService;
        private readonly IBookingRecordsManager _recordsManager;
        private readonly ILogger<BookingPaymentService> _logger;
    }
}