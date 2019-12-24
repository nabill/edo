using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public interface IBookingRequestCache
    {
        Result Set(AccommodationBookingRequest bookingRequest, string bookingReferenceCode);
        Result<AccommodationBookingRequest> Get(string bookingReferenceCode);
    }
}