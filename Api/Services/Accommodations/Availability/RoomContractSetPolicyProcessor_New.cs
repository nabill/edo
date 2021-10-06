using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    // Temporal class to replace existing RoomContractSetPolicyProcessor in further PRs
    public static class RoomContractSetPolicyProcessor_New
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
            var roomContractSetDeadline = DeadlinePolicyProcessor_New.Process(roomContractSet.Deadline, checkInDate, cancellationPolicyProcessSettings);
            var shiftedRoomContracts = new List<RoomContract>();
            foreach (var roomContract in roomContractSet.Rooms)
            {
                var roomContractDeadline = DeadlinePolicyProcessor_New.Process(roomContract.Deadline, checkInDate, cancellationPolicyProcessSettings);
                shiftedRoomContracts.Add(SetDeadline(roomContract, roomContractDeadline));
            }

            return new RoomContractSet(id: roomContractSet.Id,
                isDirectContract: roomContractSet.IsDirectContract,
                isAdvancePurchaseRate: roomContractSet.IsAdvancePurchaseRate,
                isPackageRate: roomContractSet.IsPackageRate,
                rate: roomContractSet.Rate,
                deadline: roomContractSetDeadline,
                rooms: shiftedRoomContracts,
                supplier: roomContractSet.Supplier,
                tags: roomContractSet.Tags);


            static RoomContract SetDeadline(in RoomContract roomContract, Deadline roomContractDeadline)
                => new (roomContract.BoardBasis,
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