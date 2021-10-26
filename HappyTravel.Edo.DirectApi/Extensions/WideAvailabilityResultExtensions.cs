using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.DirectApi.Models;

namespace HappyTravel.Edo.DirectApi.Extensions
{
    internal static class WideAvailabilityResultExtensions
    {
        internal static List<WideAvailabilityResult> MapFromEdoModels(this List<Api.Models.Accommodations.WideAvailabilityResult> results)
            => results.Select(r => r.MapFromEdoModel()).ToList();


        private static WideAvailabilityResult MapFromEdoModel(this Api.Models.Accommodations.WideAvailabilityResult result)
        {
            return new WideAvailabilityResult(roomContractSets: result.RoomContractSets
                .Select(rcs => new RoomContractSet(id: rcs.Id,
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
                    isPackageRate: rcs.IsPackageRate))
                .ToList(),
                minPrice: result.MinPrice,
                maxPrice: result.MaxPrice,
                checkInDate: result.CheckInDate,
                checkOutDate: result.CheckOutDate,
                supplier: result.Supplier,
                htId: result.HtId);
        }
    }
}