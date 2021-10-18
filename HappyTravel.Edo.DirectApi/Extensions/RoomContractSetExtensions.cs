using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.DirectApi.Models;

namespace HappyTravel.Edo.DirectApi.Extensions
{
    internal static class RoomContractSetExtensions
    {
        public static List<RoomContractSet> MapFromEdoModels(this List<Api.Models.Accommodations.RoomContractSet> list)
            => list.Select(rcs => rcs.MapFromEdoModel()).ToList();

        
        private static RoomContractSet MapFromEdoModel(this Api.Models.Accommodations.RoomContractSet rcs)
        {
            return new RoomContractSet(id: rcs.Id,
                rate: rcs.Rate.MapFromEdoModel(),
                deadline: rcs.Deadline,
                rooms: rcs.Rooms
                    .Select(r => new RoomContract(boardBasis: r.BoardBasis,
                        mealPlan: r.MealPlan,
                        contractTypeCode: r.ContractTypeCode,
                        isAvailableImmediately: r.IsAvailableImmediately,
                        isDynamic: r.IsDynamic,
                        contractDescription: r.ContractDescription,
                        remarks: r.Remarks,
                        dailyRoomRates: r.DailyRoomRates
                            .Select(dr => new DailyRate(fromDate: dr.FromDate,
                                toDate: dr.ToDate,
                                finalPrice: dr.FinalPrice,
                                gross: dr.Gross,
                                type: dr.Type,
                                description: dr.Description)).ToList(),
                        rate: r.Rate.MapFromEdoModel(),
                        adultsNumber: r.AdultsNumber,
                        childrenAges: r.ChildrenAges,
                        type: r.Type,
                        isExtraBedNeeded: r.IsExtraBedNeeded,
                        deadline: r.Deadline,
                        isAdvancePurchaseRate: r.IsAdvancePurchaseRate))
                    .ToList(),
                isAdvancePurchaseRate: rcs.IsAdvancePurchaseRate,
                supplier: rcs.Supplier,
                tags: rcs.Tags,
                isDirectContract: rcs.IsDirectContract,
                isPackageRate: rcs.IsPackageRate);
        }
    }
}