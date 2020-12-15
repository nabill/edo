using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface IBookingRequestCache
    {
        Task Set(string referenceCode, (AccommodationBookingRequest request, string availabilityId) requestInfo);
        
        Task<Result<(AccommodationBookingRequest request, string availabilityId)>> Get(string referenceCode);
    }
}