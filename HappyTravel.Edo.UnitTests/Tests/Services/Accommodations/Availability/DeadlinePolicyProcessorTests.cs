using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.Extensions;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Accommodations.Availability
{
    public class DeadlinePolicyProcessorTests
    {
        [Fact]
        public void Positive_shift_should_be_made_zero_in_deadline_date()
        {
            var originalDeadlineDate = new DateTimeOffset(2021, 1, 15, 0, 0, 0, TimeSpan.Zero);
            var originalDeadline = new Deadline(originalDeadlineDate, default, default, default);
            var checkIn = originalDeadlineDate;
            var shiftSpan = TimeSpan.FromDays(10);
            var settings = new CancellationPolicyProcessSettings { PolicyStartDateShift = shiftSpan };

            var shiftedDeadline = DeadlinePolicyProcessor.Process(originalDeadline, checkIn, settings);

            Assert.Equal(originalDeadline.Date, shiftedDeadline.Date);
            Assert.Equal(originalDeadline.IsFinal, shiftedDeadline.IsFinal);
            Assert.True(originalDeadline.Remarks.SafeSequenceEqual(shiftedDeadline.Remarks));
            Assert.True(originalDeadline.Policies.SafeSequenceEqual(shiftedDeadline.Policies));
        }


        [Fact]
        public void Positive_shift_should_be_made_zero_to_policies()
        {
            var originalDeadlineDate = new DateTime(2021, 1, 15);
            var shiftSpan = TimeSpan.FromDays(10);
            var originalDeadline = new Deadline(originalDeadlineDate, Policies, default, default);
            var checkIn = originalDeadlineDate;
            var settings = new CancellationPolicyProcessSettings { PolicyStartDateShift = shiftSpan };

            var shiftedDeadline = DeadlinePolicyProcessor.Process(originalDeadline, checkIn, settings);

            for(int i = 0; i < originalDeadline.Policies.Count; i++)
                Assert.Equal(originalDeadline.Policies[i].FromDate, shiftedDeadline.Policies[i].FromDate);
        }


        [Fact]
        public void Shift_should_change_date_by_exact_days_count_in_deadline_date()
        {
            var originalDeadlineDate = new DateTime(2021, 1, 15);
            var expectedShiftedDate = new DateTime(2021, 1, 5);
            var shiftSpan = TimeSpan.FromDays(-10);
            var originalDeadline = new Deadline(originalDeadlineDate, default, default, default);
            var checkIn = originalDeadlineDate;
            var settings = new CancellationPolicyProcessSettings { PolicyStartDateShift = shiftSpan };

            var shiftedDeadline = DeadlinePolicyProcessor.Process(originalDeadline, checkIn, settings);

            Assert.Equal(expectedShiftedDate, shiftedDeadline.Date);
        }


        [Fact]
        public void Shift_should_change_date_by_exact_days_count_to_policies()
        {
            var originalDeadlineDate = new DateTimeOffset(2021, 1, 15, 0, 0, 0, TimeSpan.Zero);
            var shiftSpan = TimeSpan.FromDays(-10);
            var expectedShiftedDates = new List<DateTimeOffset>
            {
                new DateTimeOffset(2021, 1, 6, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2021, 1, 7, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2021, 1, 8, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2021, 1, 9, 0, 0, 0, TimeSpan.Zero),
            };
            var originalDeadline = new Deadline(originalDeadlineDate, Policies, default, default);
            var checkIn = originalDeadlineDate;
            var settings = new CancellationPolicyProcessSettings { PolicyStartDateShift = shiftSpan };

            var shiftedDeadline = DeadlinePolicyProcessor.Process(originalDeadline, checkIn, settings);

            for (int i = 0; i < originalDeadline.Policies.Count; i++)
                Assert.Equal(expectedShiftedDates[i], shiftedDeadline.Policies[i].FromDate);
        }


        [Fact]
        public void Shift_should_keep_percentage_in_policies()
        {
            var originalDeadlineDate = new DateTime(2021, 1, 15);
            var shiftSpan = TimeSpan.FromDays(-10);
            var originalDeadline = new Deadline(originalDeadlineDate, Policies, default, default);
            var checkIn = originalDeadlineDate;
            var settings = new CancellationPolicyProcessSettings { PolicyStartDateShift = shiftSpan };

            var shiftedDeadline = DeadlinePolicyProcessor.Process(originalDeadline, checkIn, settings);

            for (int i = 0; i < originalDeadline.Policies.Count; i++)
                Assert.Equal(originalDeadline.Policies[i].Percentage, shiftedDeadline.Policies[i].Percentage);
        }


        [Fact]
        public void Shift_should_use_check_in_if_deadline_null()
        {
            var originalDeadlineDate = new DateTimeOffset(2021, 1, 15, 0, 0, 0, TimeSpan.Zero);
            var shiftSpan = TimeSpan.FromDays(0);
            var originalDeadline = new Deadline(null, default, default, default);
            var checkIn = originalDeadlineDate;
            var settings = new CancellationPolicyProcessSettings { PolicyStartDateShift = shiftSpan };

            var shiftedDeadline = DeadlinePolicyProcessor.Process(originalDeadline, checkIn, settings);

            Assert.Equal(originalDeadlineDate, shiftedDeadline.Date);
        }


        private static readonly List<CancellationPolicy> Policies = new List<CancellationPolicy>
        {
            new CancellationPolicy(new DateTimeOffset(2021, 1, 16, 0, 0, 0, TimeSpan.Zero), 1d, null),
            new CancellationPolicy(new DateTimeOffset(2021, 1, 17, 0, 0, 0, TimeSpan.Zero), 2d, null),
            new CancellationPolicy(new DateTimeOffset(2021, 1, 18, 0, 0, 0, TimeSpan.Zero), 3d, null),
            new CancellationPolicy(new DateTimeOffset(2021, 1, 19, 0, 0, 0, TimeSpan.Zero), 4d, null),
        };
    }
}
