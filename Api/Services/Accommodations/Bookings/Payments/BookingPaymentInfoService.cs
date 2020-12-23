using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments
{
    public class BookingPaymentInfoService : IBookingPaymentInfoService
    {
        public BookingPaymentInfoService(EdoContext context,
            IBookingRecordsManager bookingRecordsManager,
            ILogger<BookingPaymentInfoService> logger)
        {
            _context = context;
            _bookingRecordsManager = bookingRecordsManager;
            _logger = logger;
        }
        
        
        public async Task<Result<MoneyAmount>> GetServicePrice(string referenceCode)
        {
            var booking = await _context.Bookings.SingleOrDefaultAsync(b => b.ReferenceCode == referenceCode);
            if (booking == default)
                return Result.Failure<MoneyAmount>("Could not find booking");

            return new MoneyAmount(booking.TotalPrice, booking.Currency);
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

                    return Result.Success();
            }

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
        
        
        private readonly EdoContext _context;
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly ILogger<BookingPaymentInfoService> _logger;
    }
}