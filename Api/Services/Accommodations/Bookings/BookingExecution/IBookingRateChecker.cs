using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface IBookingRateChecker
    {
        Task<Result> Check(AccommodationBookingRequest bookingRequest, BookingAvailabilityInfo availabilityInfo, AgentContext agent);
    }
}