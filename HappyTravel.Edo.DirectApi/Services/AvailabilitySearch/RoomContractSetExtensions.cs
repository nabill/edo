using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.DirectApi.Models.Search;

namespace HappyTravel.Edo.DirectApi.Services.AvailabilitySearch
{
    internal static class RoomContractSetExtensions
    {
        public static List<RoomContractSet> MapFromEdoModels(this List<Api.Models.Accommodations.RoomContractSet> list)
            => list.Select(rcs => rcs.MapFromEdoModel()).ToList();

        
        public static RoomContractSet MapFromEdoModel(this Api.Models.Accommodations.RoomContractSet rcs)
        {
            return new RoomContractSet(id: rcs.Id,
                rate: rcs.Rate.MapFromEdoModel(),
                rooms: rcs.Rooms
                    .Select(r => new RoomContract(boardBasis: r.BoardBasis,
                        mealPlan: r.MealPlan,
                        contractTypeCode: r.ContractTypeCode.ToString(),
                        isAvailableImmediately: r.IsAvailableImmediately,
                        isDynamic: r.IsDynamic,
                        contractDescription: r.ContractDescription,
                        remarks: r.Remarks,
                        dailyRoomRates: r.DailyRoomRates
                            .Select(dr => new DailyRate(fromDate: dr.FromDate,
                                toDate: dr.ToDate,
                                totalPrice: dr.TotalPrice,
                                gross: dr.Gross,
                                type: dr.Type,
                                description: dr.Description)).ToList(),
                        rate: r.Rate.MapFromEdoModel(),
                        adultsNumber: r.AdultsNumber,
                        childrenNumber: r.ChildrenAges.Count,
                        type: r.Type,
                        deadline: r.Deadline,
                        isAdvancePurchaseRate: r.IsAdvancePurchaseRate))
                    .ToList(),
                isPackageRate: rcs.IsPackageRate);
        }
    }
}