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
            var roomContractSetDeadline = DeadlinePolicyProcessor.Process(roomContractSet.Deadline, checkInDate, cancellationPolicyProcessSettings);
            var shiftedRoomContracts = new List<RoomContract>();
            foreach (var roomContract in roomContractSet.RoomContracts)
            {
                var roomContractDeadline = DeadlinePolicyProcessor.Process(roomContract.Deadline, checkInDate, cancellationPolicyProcessSettings);
                shiftedRoomContracts.Add(SetDeadline(roomContract, roomContractDeadline));
            }

            return new RoomContractSet(roomContractSet.Id, 
                    roomContractSet.Rate, 
                    roomContractSetDeadline, 
                    shiftedRoomContracts, 
                    roomContractSet.Tags, 
                    isDirectContract: roomContractSet.IsDirectContract,
                    isAdvancePurchaseRate: roomContractSet.IsAdvancePurchaseRate,
                    isPackageRate: roomContractSet.IsPackageRate
                );


            static RoomContract SetDeadline(in RoomContract roomContract, Deadline roomContractDeadline)
                => new(roomContract.BoardBasis,
                    roomContract.MealPlan,
                    roomContract.ContractTypeCode,
                    isAvailableImmediately: roomContract.IsAvailableImmediately,
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
                    isAdvancePurchaseRate: roomContract.IsAdvancePurchaseRate
                );
        }
    }
}