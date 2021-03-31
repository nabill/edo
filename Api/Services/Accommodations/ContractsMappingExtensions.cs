using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public static class ContractsMappingExtensions
    {
        public static RoomContractSet ToRoomContractSet(this in EdoContracts.Accommodations.Internals.RoomContractSet roomContractSet, Suppliers? supplier, List<string> tags)
        {
            return new RoomContractSet(roomContractSet.Id,
                roomContractSet.Rate.ToRate(),
                roomContractSet.Deadline.ToDeadline(),
                roomContractSet.RoomContracts.ToRoomContractList(),
                roomContractSet.IsAdvancePurchaseRate,
                supplier,
                tags,
                roomContractSet.IsDirectContract);
        }

        
        public static Deadline ToDeadline(this in EdoContracts.Accommodations.Deadline deadline)
        {
            return new (deadline.Date, deadline.Policies.ToPolicyList(), deadline.Remarks, deadline.IsFinal);
        }


        private static List<CancellationPolicy> ToPolicyList(this IEnumerable<EdoContracts.Accommodations.Internals.CancellationPolicy> policies)
        {
            return policies
                .Select(ToCancellationPolicy)
                .ToList();
        }
        

        private static RoomContract ToRoomContract(this EdoContracts.Accommodations.Internals.RoomContract roomContract)
        {
            return new RoomContract(roomContract.BoardBasis, roomContract.MealPlan,
                roomContract.ContractTypeCode, roomContract.IsAvailableImmediately, roomContract.IsDynamic,
                roomContract.ContractDescription, roomContract.Remarks, roomContract.DailyRoomRates.ToDailyRateList(), roomContract.Rate.ToRate(),
                roomContract.AdultsNumber, roomContract.ChildrenAges, roomContract.Type, roomContract.IsExtraBedNeeded,
                roomContract.Deadline.ToDeadline(), roomContract.IsAdvancePurchaseRate);
        }
        
        
        private static Rate ToRate(this EdoContracts.General.Rate rate)
        {
            return new (rate.FinalPrice, rate.Gross, rate.Discounts, rate.Type, rate.Description);
        }


        private static CancellationPolicy ToCancellationPolicy(EdoContracts.Accommodations.Internals.CancellationPolicy policy)
        {
            return new (policy.FromDate, policy.Percentage);
        }
        
        
        private static List<DailyRate> ToDailyRateList(this IEnumerable<EdoContracts.General.DailyRate> rates)
        {
            return rates
                .Select(r => new DailyRate(r.FromDate,
                    r.ToDate,
                    r.FinalPrice,
                    r.Gross,
                    r.Type,
                    r.Description))
                .ToList();
        }
        
        
        private static List<RoomContract> ToRoomContractList(
            this IEnumerable<EdoContracts.Accommodations.Internals.RoomContract> roomContractSets)
        {
            return roomContractSets
                .Select(ToRoomContract)
                .ToList();
        }
    }
}