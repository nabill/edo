using System.Linq;
using HappyTravel.Edo.Api.AdministratorServices.Models;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IBookingService
    {
        IQueryable<BookingSlim> GetAllBookings();
        IQueryable<BookingSlim> GetAgencyBookings(int agencyId);
        IQueryable<BookingSlim> GetAgentBookings(int agentId);
    }
}
