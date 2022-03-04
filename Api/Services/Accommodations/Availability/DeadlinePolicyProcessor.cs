using System;
using System.Collections.Generic;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public static class DeadlinePolicyProcessor
    {
        public static Deadline Process(Deadline deadline, DateTimeOffset checkInDate, CancellationPolicyProcessSettings cancellationPolicyProcessSettings)
        {
            var shiftValue = cancellationPolicyProcessSettings.PolicyStartDateShift;
            // This value cannot be positive because we cannot shift deadlines to future, only to the past
            shiftValue = shiftValue > TimeSpan.Zero
                ? TimeSpan.Zero
                : shiftValue;

            if (deadline.Date == default(DateTimeOffset))
                throw new ArgumentException($"Deadline date '{deadline.Date}' is invalid", nameof(deadline.Date));
            
            if (checkInDate == default)
                throw new ArgumentException($"Check in date '{checkInDate}' is invalid", nameof(checkInDate));
                
            var shiftedDeadlineDate = ShiftDate(deadline.Date, checkInDate, shiftValue);
            List<CancellationPolicy> shiftedPolicies = new List<CancellationPolicy>();
            foreach (var policy in deadline.Policies)
            {
                if (policy.FromDate == default)
                    throw new ArgumentException($"Policy date '{policy.FromDate}' is invalid", nameof(policy.FromDate));
                
                var shiftedPolicyDate = ShiftDate(policy.FromDate, checkInDate, shiftValue);
                var percentage = Math.Round(policy.Percentage, 2, MidpointRounding.AwayFromZero);
                shiftedPolicies.Add(new CancellationPolicy(shiftedPolicyDate, percentage, policy.Remark));
            }

            var shiftedDeadline = new Deadline(shiftedDeadlineDate, shiftedPolicies, deadline.Remarks, deadline.IsFinal);
            return shiftedDeadline;
        }


        private static DateTimeOffset ShiftDate(DateTimeOffset? current, DateTimeOffset checkInDate, TimeSpan shiftValue)
            => (current ?? checkInDate) + shiftValue;
    }
}