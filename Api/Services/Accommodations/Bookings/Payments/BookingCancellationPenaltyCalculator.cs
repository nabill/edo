using System;
using System.Linq;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Money.Helpers;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments
{
    public static class BookingCancellationPenaltyCalculator
    {
        public static MoneyAmount Calculate(Booking booking, DateTimeOffset cancellationDate)
        {
            var penaltyAmount = booking.Rooms
                .Sum(room => room.Price.Amount * (decimal) GetPenaltyFraction(room.DeadlineDetails, room.IsAdvancePurchaseRate, cancellationDate));

            penaltyAmount = MoneyRounder.Truncate(penaltyAmount, booking.Currency);
            
            return new MoneyAmount(penaltyAmount, booking.Currency);

            static double GetPenaltyFraction(Deadline deadline, bool isAdvancePurchaseRate, DateTimeOffset cancellationDate)
            {
                if (isAdvancePurchaseRate)
                    return Whole;
                
                if (!deadline.Policies.Any())
                    return cancellationDate >= deadline.Date // Booking was cancelled after deadline
                        ? Whole
                        : Nothing;

                if (deadline.Policies.Any(p => p.Percentage > 100))
                    throw new Exception("Percentage cannot be greater then 100");

                var appliedPolicy = deadline.Policies
                    .OrderBy(p => p.FromDate)
                    .LastOrDefault(p => p.FromDate <= cancellationDate);

                return appliedPolicy?.Percentage / 100 ?? Nothing;
            }
        }
        
        
        private const double Whole = 1d;
        private const double Nothing = 0d;
    }
}