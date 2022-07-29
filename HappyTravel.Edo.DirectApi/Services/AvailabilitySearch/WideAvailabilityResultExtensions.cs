using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.DirectApi.Models;
using HappyTravel.Edo.DirectApi.Models.Search;

namespace HappyTravel.Edo.DirectApi.Services.AvailabilitySearch
{
    internal static class WideAvailabilityResultExtensions
    {
        internal static List<WideAvailabilityResult> MapFromEdoModels(this List<Api.Models.Accommodations.WideAvailabilityResult> results)
            => results.Select(r => r.MapFromEdoModel()).ToList();


        private static WideAvailabilityResult MapFromEdoModel(this Api.Models.Accommodations.WideAvailabilityResult result)
        {
            return new WideAvailabilityResult(accommodationId: result.HtId, 
                roomContractSets: result.RoomContractSets.MapFromEdoModels(),
                checkInDate: result.CheckInDate,
                checkOutDate: result.CheckOutDate,
                expiredAfter: result.ExpiredAfter);
        }
    }
}