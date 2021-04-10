using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.Extensions;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Accommodations.Availability
{
    public class DeadlinePolicyProcessorTests
    {
        [Fact]
        public void Positive_shift_should_be_made_zero()
        {
            var originalDeadline = new Deadline(FromDate);
            var checkIn = FromDate;
            var shiftSpan = TimeSpan.FromDays(10);
            var settings = new CancellationPolicyProcessSettings { PolicyStartDateShift = shiftSpan };

            var shiftedDeadline = DeadlinePolicyProcessor.Process(originalDeadline, checkIn, settings);

            Assert.Equal(originalDeadline, shiftedDeadline);
        }


        [Fact]
        public void Positive_shift_should_be_made_zero_to_policies()
        {

            var originalDeadline = new Deadline(FromDate, Policies);
            var checkIn = FromDate;
            var shiftSpan = TimeSpan.FromDays(10);
            var settings = new CancellationPolicyProcessSettings { PolicyStartDateShift = shiftSpan };

            var shiftedDeadline = DeadlinePolicyProcessor.Process(originalDeadline, checkIn, settings);
            
            Assert.True(originalDeadline.Policies.Select(p => p.FromDate)
                .SafeSequenceEqual(shiftedDeadline.Policies.Select(p => p.FromDate)));
        }


        [Fact]
        public void Shift_should_change_by_exact_amount()
        {
            var originalDeadline = new Deadline(FromDate);
            var checkIn = FromDate;
            var shiftSpan = TimeSpan.FromDays(-10);
            var settings = new CancellationPolicyProcessSettings { PolicyStartDateShift = shiftSpan };

            var shiftedDeadline = DeadlinePolicyProcessor.Process(originalDeadline, checkIn, settings);

            Assert.Equal(originalDeadline.Date + shiftSpan, shiftedDeadline.Date);
        }


        [Fact]
        public void Shift_should_change_by_exact_amount_to_policies()
        {
            var originalDeadline = new Deadline(FromDate, Policies);
            var checkIn = FromDate;
            var shiftSpan = TimeSpan.FromDays(-10);
            var settings = new CancellationPolicyProcessSettings { PolicyStartDateShift = shiftSpan };

            var shiftedDeadline = DeadlinePolicyProcessor.Process(originalDeadline, checkIn, settings);

            Assert.True(originalDeadline.Policies.Select(p => p.FromDate + shiftSpan)
                .SafeSequenceEqual(shiftedDeadline.Policies.Select(p => p.FromDate)));
        }


        [Fact]
        public void Shift_should_keep_percentage_in_policies()
        {
            var originalDeadline = new Deadline(FromDate, Policies);
            var checkIn = FromDate;
            var shiftSpan = TimeSpan.FromDays(-10);
            var settings = new CancellationPolicyProcessSettings { PolicyStartDateShift = shiftSpan };

            var shiftedDeadline = DeadlinePolicyProcessor.Process(originalDeadline, checkIn, settings);

            Assert.True(originalDeadline.Policies.Select(p => p.Percentage)
                .SafeSequenceEqual(shiftedDeadline.Policies.Select(p => p.Percentage)));
        }


        [Fact]
        public void Shift_should_use_check_in_if_deadline_null()
        {
            var originalDeadline = new Deadline(null);
            var checkIn = FromDate;
            var shiftSpan = TimeSpan.FromDays(0);
            var settings = new CancellationPolicyProcessSettings { PolicyStartDateShift = shiftSpan };

            var shiftedDeadline = DeadlinePolicyProcessor.Process(originalDeadline, checkIn, settings);

            Assert.Equal(FromDate, shiftedDeadline.Date);
        }


        private static readonly DateTime FromDate = new DateTime(2021, 1, 1);

        private static readonly List<CancellationPolicy> Policies = new List<CancellationPolicy>
        {
            new CancellationPolicy(FromDate + TimeSpan.FromDays(1), 1d),
            new CancellationPolicy(FromDate + TimeSpan.FromDays(2), 2d),
            new CancellationPolicy(FromDate + TimeSpan.FromDays(3), 3d),
            new CancellationPolicy(FromDate + TimeSpan.FromDays(4), 4d),
        };
    }
}
