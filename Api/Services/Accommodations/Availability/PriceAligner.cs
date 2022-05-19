using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Money.Extensions;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public static class PriceAligner
    {
        /// <summary>
        /// Aligns full price with its containing parts
        /// </summary>
        /// <param name="aggregated">Full price</param>
        /// <param name="parts">The parts of full price which sum must be equal to the full price</param>
        /// <returns>Full price and its parts</returns>
        /// <example>
        /// Aggregated price - 100 USD
        /// Parts - 50 USD + 49 USD
        ///
        /// Result will contain 100 USD as full price (aggregated) and 50 USD + 50 USD as parts.
        /// </example>
        /// <exception cref="NotSupportedException">When aggregated price and its parts differ too much. This may be a sign of an error in client code</exception>
        public static (MoneyAmount Aggregated, List<MoneyAmount> Parts) AlignAggregatedValues(MoneyAmount aggregated, List<MoneyAmount> parts)
        {
            if (parts.Any(p => p.Currency != aggregated.Currency))
                throw new NotSupportedException($"Aggregated value and parts value currency mismatch");

            var partsSum = new MoneyAmount(parts.Sum(p => p.Amount), aggregated.Currency);
            if (Math.Abs(aggregated.Amount - partsSum.Amount) > MaxSupportedAggregatePriceDifferenceThreshold)
                throw new NotSupportedException($"Aggregated value {aggregated.Amount} differs from aggregate sum {partsSum.Amount} for mor than allowed threshold {MaxSupportedAggregatePriceDifferenceThreshold}");

            return aggregated switch
            {
                _ when aggregated == partsSum => (aggregated, parts),
                _ when aggregated < partsSum => (partsSum, parts),
                _ when aggregated > partsSum => Align(aggregated, parts),
                _ => throw new ArgumentOutOfRangeException(nameof(aggregated), aggregated, null)
            };


            static (MoneyAmount Aggregated, List<MoneyAmount> Parts) Align(MoneyAmount aggregated, List<MoneyAmount> parts)
            {
                var decimalDigitsCount = aggregated.Currency.GetDecimalDigitsCount();
                var changeStep = (decimal)Math.Pow(0.1, decimalDigitsCount);

                // When aggregated value is larger than sum of its parts, we increase each part amount until their sum will reach the aggregated amount.
                // All parts amounts increased at once to preserve their price difference, which is especially important for cases when same service is repeated in one order.
                while (parts.Sum(p => p.Amount) < aggregated.Amount)
                {
                    parts = parts
                        .Select(p => new MoneyAmount(p.Amount + changeStep, p.Currency))
                        .ToList();
                }

                return (new MoneyAmount(parts.Sum(p => p.Amount), aggregated.Currency), parts);
            }
        }


        private const decimal MaxSupportedAggregatePriceDifferenceThreshold = (decimal)0.5;
    }
}