using System;
using System.Linq;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.Money.Helpers;
using Booking = HappyTravel.Edo.Data.Booking.Booking;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class BookingExtensions
    {
        public static decimal GetRefundableAmount(this Booking booking, DateTime forDate)
        {
            var refundableAmount = booking.Rooms.Sum(room => room.Price.Amount * (decimal)room.DeadlineDetails.GetRefundableFraction(forDate));
            return MoneyRounder.Ceil(refundableAmount, booking.Currency);
        }


        static double GetRefundableFraction(this Deadline deadline, DateTime forDate) =>
            1d - deadline.GetNonRefundableFraction(forDate);


        static double GetNonRefundableFraction(this Deadline deadline, DateTime forDate)
        {
            if (!deadline.Policies.Any())
                return deadline.Date <= forDate
                    ? Whole
                    : Nothing;

            var appliedPolicy = deadline.Policies.OrderBy(p => p.FromDate).LastOrDefault(p => p.FromDate <= forDate);

            return appliedPolicy.Equals(default)
                ? Nothing
                : appliedPolicy.Percentage;
        }

        private const double Whole = 1d;
        private const double Nothing = 0d;
    }
}
