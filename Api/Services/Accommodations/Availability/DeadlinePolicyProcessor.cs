using System;
using System.Collections.Generic;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public static class DeadlinePolicyProcessor
    {
        public static Deadline Process(Deadline deadline, DateTime checkInDate, CancellationPolicyProcessSettings cancellationPolicyProcessSettings)
        {
            var shiftValue = cancellationPolicyProcessSettings.PolicyStartDateShift;
            // This value cannot be positive because we cannot shift deadlines to future, only to the past
            shiftValue = shiftValue > TimeSpan.Zero
                ? TimeSpan.Zero
                : shiftValue;

            var shiftedDeadlineDate = ShiftDate(deadline.Date, checkInDate, shiftValue);
            List<CancellationPolicy> shiftedPolicies = new List<CancellationPolicy>();
            foreach (var policy in deadline.Policies)
            {
                var shiftedPolicyDate = ShiftDate(policy.FromDate, policy.FromDate, shiftValue);
                shiftedPolicies.Add(new CancellationPolicy(shiftedPolicyDate, policy.Percentage));
            }

            var shiftedDeadline = new Deadline(shiftedDeadlineDate, shiftedPolicies, deadline.Remarks, deadline.IsFinal);
            return shiftedDeadline;
        }


        private static DateTime ShiftDate(DateTime? current, DateTime checkInDate, TimeSpan shiftValue)
            => (current ?? checkInDate) + shiftValue;
    }
}