using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution
{
    public interface IBookingRequestStorage
    {
        Task Set(string referenceCode, AccommodationBookingRequest request, BookingAvailabilityInfo availabilityInfo);
        
        Task<Result<(AccommodationBookingRequest request, BookingAvailabilityInfo availabilityInfo)>> Get(string referenceCode);
    }
}