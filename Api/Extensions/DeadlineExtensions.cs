using System;
using System.Linq;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class DeadlineExtensions
    {
        private const double Whole = 1d;
        private const double Nothing = 0d;

        public static double GetRefundableFraction(this Deadline deadline, DateTime forDate) => 
            1d - deadline.GetNonRefundableFraction(forDate);

        public static double GetNonRefundableFraction(this Deadline deadline, DateTime forDate)
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
    }
}
