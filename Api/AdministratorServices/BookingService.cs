using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using System.Linq;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class BookingService : IBookingService
    {
        public BookingService(EdoContext context)
        {
            _context = context;
        }


        public IQueryable<Booking> GetAgencyBookings(int agencyId) 
            => _context.Bookings.Where(booking => booking.AgencyId == agencyId);


        public IQueryable<Booking> GetAgentBookings(int agentId) 
            => _context.Bookings.Where(booking => booking.AgentId == agentId);


        private readonly EdoContext _context;
    }
}
