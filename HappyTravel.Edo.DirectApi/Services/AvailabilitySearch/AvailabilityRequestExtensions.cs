using HappyTravel.Edo.Api.Models.Availabilities;
using AvailabilityRequest = HappyTravel.Edo.DirectApi.Models.AvailabilityRequest;

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
                roomDetails: request.RoomDetails,
                ratings: default,
                propertyTypes: default,
                htIds: request.Ids);
        }
    }
}