using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Accommodations
{
    public class PriceAlignTests
    {
        [Fact]
        public void Too_much_differing_values_should_fail()
        {
            var aggregated = new MoneyAmount(100, Currencies.USD);
            var parts = new List<MoneyAmount>()
            {
                new (40, Currencies.USD),
                new (30, Currencies.USD),
            };

            Assert.Throws<NotSupportedException>(() => PriceAligner.AlignAggregateValues(aggregated, parts));
        }


        [Fact]
        public void Should_fail_on_currency_mismatch()
        {
            var aggregated = new MoneyAmount(100, Currencies.AED);
            var parts = new List<MoneyAmount>()
            {
                new (50, Currencies.USD),
                new (50, Currencies.USD),
            };

            Assert.Throws<NotSupportedException>(() => PriceAligner.AlignAggregateValues(aggregated, parts));
        }
        
        
        [Theory]
        [InlineData(100.1, 50.1, 50)]
        [InlineData(12467.01, 1000, 11467.01)]
        [InlineData(0.02, 0.01, 0.01)]
        public void Should_return_same_value_if_align_is_not_needed(decimal aggregated, decimal part1, decimal part2)
        {
            var aggregatedAmount = new MoneyAmount(aggregated, Currencies.USD);
            var partAmounts = new List<MoneyAmount>
            {
                new (part1, Currencies.USD),
                new (part2, Currencies.USD),
            };

            var aligned = PriceAligner.AlignAggregateValues(aggregatedAmount, partAmounts);
            

            Assert.Equal(aggregatedAmount, aligned.Aggregated);
            Assert.Equal(partAmounts, aligned.Parts);
        }
        
        
        [Theory]
        [InlineData(100, 50.1, 50, 100.1)] // Increase full price to match parts
        [InlineData(12467.1, 1000, 11467.01, 12467.11)] // Increase parts to match parts
        [InlineData(906.73, 453.37, 453.37, 906.74)] // Increase parts to match parts
        [InlineData(906.75, 453.37, 453.37, 906.76)] // Increase parts to match parts
        public void Should_align_to_the_largest(decimal aggregated, decimal part1, decimal part2, decimal expectedAggregated)
        {
            var aggregatedAmount = new MoneyAmount(aggregated, Currencies.USD);
            var partAmounts = new List<MoneyAmount>
            {
                new (part1, Currencies.USD),
                new (part2, Currencies.USD),
            };

            var aligned = PriceAligner.AlignAggregateValues(aggregatedAmount, partAmounts);
            
            Assert.Equal(expectedAggregated, aligned.Aggregated.Amount);
        }
        
        
        [Theory]
        [InlineData(149.5, 49.9)] // Increase full price to match parts
        [InlineData(1360.15, 453.37)] // Increase parts to match parts
        public void Should_preserve_same_part_prices_as_same(decimal aggregated, decimal partAmount)
        {
            var aggregatedAmount = new MoneyAmount(aggregated, Currencies.USD);
            var partAmountsWithSamePrice = new List<MoneyAmount>
            {
                new (partAmount, Currencies.USD),
                new (partAmount, Currencies.USD),
                new (partAmount, Currencies.USD)
            };

            var aligned = PriceAligner.AlignAggregateValues(aggregatedAmount, partAmountsWithSamePrice);
            var alignedParts = aligned.Parts;

            Assert.Equal(alignedParts[0], alignedParts[1]);
            Assert.Equal(alignedParts[1], alignedParts[2]);
        }
    }
}