using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public static class RoomContractSetExtensions
    {
        public static RoomContractSet WithEmptyTags(this RoomContractSet rs)
            => WithTags(rs, new List<string>(0));
        
        private static RoomContractSet WithTags(this RoomContractSet rs, List<string> tags)
            => new (rs.Id,
                rs.Rate,
                rs.Deadline,
                rs.Rooms,
                rs.IsAdvancePurchaseRate,
                rs.Supplier,
                tags);
    }
}