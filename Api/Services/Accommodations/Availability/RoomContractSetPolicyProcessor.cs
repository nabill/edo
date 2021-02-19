using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public static class RoomContractSetPolicyProcessor
    {
        public static List<RoomContractSet> Process(List<RoomContractSet> roomContractSet, DateTime checkInDate, CancellationPolicyProcessSettings cancellationPolicyProcessSettings)
        {
            return roomContractSet
                .Select(r => Process(r, checkInDate, cancellationPolicyProcessSettings))
                .ToList();
        }
        

        public static RoomContractSet Process(RoomContractSet roomContractSet, DateTime checkInDate,
            CancellationPolicyProcessSettings cancellationPolicyProcessSettings)
        {
            var shiftValue = cancellationPolicyProcessSettings.PolicyStartDateShift;
            // This value cannot be positive because we cannot shift deadlines to future, only to the past
            shiftValue = shiftValue > TimeSpan.Zero
                ? TimeSpan.Zero
                : shiftValue;
            
            var roomContractSetDeadline = GetShiftedDeadline(roomContractSet.Deadline, checkInDate, shiftValue);
            var shiftedRoomContracts = new List<RoomContract>();
            foreach (var roomContract in roomContractSet.RoomContracts)
            {
                var roomContractDeadline = GetShiftedDeadline(roomContract.Deadline, checkInDate, shiftValue);
                shiftedRoomContracts.Add(SetDeadline(roomContract, roomContractDeadline));
            }

            return new RoomContractSet(roomContractSet.Id, roomContractSet.Rate, roomContractSetDeadline, shiftedRoomContracts, roomContractSet.IsAdvancePurchaseRate);


            static RoomContract SetDeadline(in RoomContract roomContract, Deadline roomContractDeadline)
                => new(roomContract.BoardBasis,
                    roomContract.MealPlan,
                    roomContract.ContractTypeCode,
                    roomContract.IsAvailableImmediately,
                    roomContract.IsDynamic,
                    roomContract.ContractDescription,
                    roomContract.Remarks,
                    roomContract.DailyRoomRates,
                    roomContract.Rate,
                    roomContract.AdultsNumber,
                    roomContract.ChildrenAges,
                    roomContract.Type,
                    roomContract.IsExtraBedNeeded,
                    roomContractDeadline,
                    roomContract.IsAdvancePurchaseRate
                );
        }


        private static Deadline GetShiftedDeadline(Deadline deadline, DateTime checkInDate, TimeSpan shiftValue)
        {
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