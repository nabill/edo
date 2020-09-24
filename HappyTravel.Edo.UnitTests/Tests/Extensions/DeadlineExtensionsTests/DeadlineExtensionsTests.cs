using System;
using System.Collections.Generic;
using System.Text;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Extensions.DeadlineExtensionsTests
{
    public class DeadlineExtensionsTests
    {
        [Fact]
        public void Everything_unknown_should_refund_full_price()
        {
            var deadline = new Deadline(
                date: null,
                policies: null);
            var forDate = new DateTime(2020, 1, 1);

            var refundableFraction = deadline.GetRefundableFraction(forDate);

            Assert.Equal(1d, refundableFraction);
        }


        [Fact]
        public void No_policies_before_deadline_should_refund_full_price()
        {
            var deadline = new Deadline(
                date: new DateTime(2020, 1, 2),
                policies: null);
            var forDate = new DateTime(2020, 1, 1);

            var refundableFraction = deadline.GetRefundableFraction(forDate);

            Assert.Equal(1d, refundableFraction);
        }


        [Fact]
        public void No_policies_after_deadline_should_refund_nothing()
        {
            var deadline = new Deadline(
                date: new DateTime(2020, 1, 1),
                policies: null);
            var forDate = new DateTime(2020, 1, 2);

            var refundableFraction = deadline.GetRefundableFraction(forDate);

            Assert.Equal(0d, refundableFraction);
        }


        [Fact]
        public void Existing_policies_should_ignore_deadline_date()
        {
            var deadline = new Deadline(
                date: new DateTime(2020, 1, 1),
                policies: new List<CancellationPolicy>
                {
                    new CancellationPolicy(fromDate: new DateTime(2020, 1, 3), 0.3d)
                });
            var forDate = new DateTime(2020, 1, 2);

            var refundableFraction = deadline.GetRefundableFraction(forDate);

            Assert.Equal(1d, refundableFraction);
        }


        [Theory]
        [InlineData(2, 1.0d)]
        [InlineData(3, 0.7d)]
        [InlineData(4, 0.7d)]
        [InlineData(5, 0.4d)]
        [InlineData(6, 0.4d)]
        [InlineData(7, 0.4d)]
        [InlineData(8, 0.2d)]
        [InlineData(9, 0.2d)]
        public void Existing_policies_should_show_refund_according_date(int day, double expectedRefundableFraction)
        {
            var deadline = new Deadline(
                date: new DateTime(2020, 1, 3),
                policies: new List<CancellationPolicy>
                {
                    new CancellationPolicy(fromDate: new DateTime(2020, 1, 3), 0.3d),
                    new CancellationPolicy(fromDate: new DateTime(2020, 1, 5), 0.6d),
                    new CancellationPolicy(fromDate: new DateTime(2020, 1, 8), 0.8d),
                });
            var forDate = new DateTime(2020, 1, day);

            var refundableFraction = deadline.GetRefundableFraction(forDate);
            
            Assert.True(EqualsWithTolerance(expectedRefundableFraction, refundableFraction));
        }


        bool EqualsWithTolerance(double a, double b) => Math.Abs(a - b) < 0.000000001d;
    }
}
