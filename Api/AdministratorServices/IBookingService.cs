using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.AdministratorServices.Models;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IBookingService
    {
        IQueryable<BookingSlim> GetAllBookings();
        IQueryable<BookingSlim> GetAgencyBookings(int agencyId);
        IQueryable<BookingSlim> GetAgentBookings(int agentId);
        Task NormalizeBookingsPrices();
    }
}
