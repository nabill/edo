using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution
{
    public interface IBookingRequestExecutor
    {
        Task<Booking> Execute(AccommodationBookingRequest bookingRequest, string availabilityId, Data.Bookings.Booking booking, AgentContext agent, string languageCode);
    }
}