using System.Linq;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.EdoContracts.Accommodations.Internals;
using AvailabilityRequest = HappyTravel.Edo.DirectApi.Models.Search.AvailabilityRequest;

namespace HappyTravel.Edo.DirectApi.Services.AvailabilitySearch
{
    public static class AvailabilityRequestExtensions
    {
        internal static Api.Models.Availabilities.AvailabilityRequest ToEdoModel(this AvailabilityRequest request)
        {
            return new Api.Models.Availabilities.AvailabilityRequest(nationality: request.Nationality.ToUpper(),
                residency: request.Residency.ToUpper(),
                checkInDate: request.CheckInDate,
                checkOutDate: request.CheckOutDate,
                filters: ClientSearchFilters.AvailableOnly,
                roomDetails: request.RoomDetails
                    .Select(r => new RoomOccupationRequest(r.AdultsNumber, r.ChildrenAges))
                    .ToList(),
                ratings: default,
                propertyTypes: default,
                htIds: request.Ids);
        }
    }
}