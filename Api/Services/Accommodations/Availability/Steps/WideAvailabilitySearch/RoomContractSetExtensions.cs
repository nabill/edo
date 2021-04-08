using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch
{
    public static class RoomContractSetExtensions
    {
        public static RoomContractSet WithFalseDirectContractsFlag(this RoomContractSet rs) 
            => new(rs.Id,
            rs.Rate,
            rs.Deadline,
            rs.Rooms,
            rs.IsAdvancePurchaseRate,
            rs.Supplier,
            rs.Tags,
            isDirectContract: false);
    }
}