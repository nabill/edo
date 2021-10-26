using HappyTravel.Edo.DirectApi.Models;

namespace HappyTravel.Edo.DirectApi.Extensions
{
    public static class AvailabilityRequestExtensions
    {
        internal static Api.Models.Availabilities.AvailabilityRequest ToEdoModel(this AvailabilityRequest request)
        {
            return new Api.Models.Availabilities.AvailabilityRequest(nationality: request.Nationality,
                residency: request.Residency,
                checkInDate: request.CheckInDate,
                checkOutDate: request.CheckOutDate,
                filters: request.Filters,
                roomDetails: request.RoomDetails,
                ratings: request.Ratings,
                propertyTypes: request.PropertyType,
                htIds: request.HtIds);
        }
    }
}