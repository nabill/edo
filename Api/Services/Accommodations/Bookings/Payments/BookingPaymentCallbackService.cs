using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments
{
    public class BookingPaymentCallbackService : IBookingPaymentCallbackService
    {
        public BookingPaymentCallbackService(EdoContext context,
            IBookingRecordsManager bookingRecordsManager,
            IDateTimeProvider dateTimeProvider,
            ILogger<BookingPaymentCallbackService> logger)
        {
            _context = context;
            _bookingRecordsManager = bookingRecordsManager;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }
        
        
        public async Task<Result<MoneyAmount>> GetServicePrice(string referenceCode)
        {
            var (_, isFailure, booking, error) = await _bookingRecordsManager.Get(referenceCode);
            if (isFailure)
                return Result.Failure<MoneyAmount>(error);

            return new MoneyAmount(booking.TotalPrice, booking.Currency);
        }


        public async Task<Result<MoneyAmount>> GetRefundableAmount(string referenceCode)
        {
            var (_, isFailure, booking, error) = await _bookingRecordsManager.Get(referenceCode);
            if (isFailure)
                return Result.Failure<MoneyAmount>(error);

            if (booking.Status == BookingStatuses.Rejected || booking.Status == BookingStatuses.Discarded)
                return new MoneyAmount(booking.TotalPrice, booking.Currency);

            return new MoneyAmount(booking.GetRefundableAmount(_dateTimeProvider.UtcToday().AddDays(BookingConstants.DaysBeforeDeadlineWhenToPay)), booking.Currency);
        }


        public async Task<Result> ProcessPaymentChanges(Payment payment)
        {
            var (_, isFailure, booking, error) = await _bookingRecordsManager.Get(payment.ReferenceCode);
            if (isFailure)
            {
                _logger.LogProcessPaymentChangesForBookingFailure("Failed to process payment changes, " +
                    $"could not find the corresponding booking. Payment status: {payment.Status}. Payment: '{payment.ReferenceCode}'");

                return Result.Failure($"Could not find booking for payment '{error}'");
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

                    return Result.Success();
            }

            booking.PaymentMethod = payment.PaymentMethod;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            
            _context.Entry(booking).State = EntityState.Detached;

            _logger.LogProcessPaymentChangesForBookingSuccess($"Successfully processes payment changes. Old payment status: {oldPaymentStatus}. " +
                $"New payment status: {payment.Status}. Payment: '{payment.ReferenceCode}'. Booking reference code: '{booking.ReferenceCode}'");

            return Result.Success();
        }


        public async Task<Result<(int AgentId, int AgencyId)>> GetServiceBuyer(string referenceCode)
        {
            var (_, isFailure, booking, error) = await _bookingRecordsManager.Get(referenceCode);
            if (isFailure)
                return Result.Failure<(int, int)>(error);

            return (booking.AgentId, booking.AgencyId);
        }


        public async Task<Result<int>> GetChargingAccountId(string referenceCode)
        {
            var (_, isFailure, booking, error) = await _bookingRecordsManager.Get(referenceCode);
            if (isFailure)
                return Result.Failure<int>(error);

            if (booking.PaymentMethod != PaymentMethods.BankTransfer)
                return Result.Failure<int>("Invalid payment method");

            var account = await _context.AgencyAccounts.SingleOrDefaultAsync(a => a.AgencyId == booking.AgencyId && a.Currency == booking.Currency);
            if (account is null)
                return Result.Failure<int>($"Could not get agency account for booking {referenceCode}");
            
            _context.Entry(account).State = EntityState.Detached;
            return account.Id;
        }


        private readonly EdoContext _context;
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<BookingPaymentCallbackService> _logger;
    }
}