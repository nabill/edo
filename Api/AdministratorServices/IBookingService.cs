using HappyTravel.Edo.Data.Bookings;
using System.Linq;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IBookingService
    {
        IQueryable<Booking> GetAgencyBookings(int agencyId);
        IQueryable<Booking> GetAgentBookings(int agentId);
    }
}
