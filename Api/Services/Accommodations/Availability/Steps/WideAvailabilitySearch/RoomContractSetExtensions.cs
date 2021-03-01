using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public static class RoomContractSetExtensions
    {
        public static RoomContractSet WithEmptySystemTags(this RoomContractSet rs)
            => WithSystemTags(rs, new List<string>(0));
        
        private static RoomContractSet WithSystemTags(this RoomContractSet rs, List<string> systemTags)
            => new (rs.Id,
                rs.Rate,
                rs.Deadline,
                rs.Rooms,
                rs.IsAdvancePurchaseRate,
                rs.Supplier,
                systemTags);
    }
}