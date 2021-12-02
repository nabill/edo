using System;
using HappyTravel.Edo.Data;
using System.Linq;
using System.Linq.Expressions;
using HappyTravel.Edo.Api.AdministratorServices.Models;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class BookingService : IBookingService
    {
        public BookingService(EdoContext context)
        {
            _context = context;
        }


        public IQueryable<BookingSlim> GetAllBookings() 
            => GetBookings();


        public IQueryable<BookingSlim> GetAgencyBookings(int agencyId) 
            => GetBookings(booking => booking.AgencyId == agencyId);


        public IQueryable<BookingSlim> GetAgentBookings(int agentId) 
            => GetBookings(booking => booking.AgentId == agentId);


        private IQueryable<BookingSlim> GetBookings(Expression<Func<BookingSlim, bool>>? expression = null)
        {
            var query = _context.Bookings
                .Select(b => new BookingSlim
                {
                    Id = b.Id,
                    ReferenceCode = b.ReferenceCode,
                    AccommodationName = b.AccommodationName,
                    AgencyId = b.AgencyId,
                    AgentId = b.AgentId,
                    Created = b.Created,
                    Currency = b.Currency,
                    PaymentStatus = b.PaymentStatus,
                    TotalPrice = b.TotalPrice
                });

            return expression == null
                ? query
                : query.Where(expression);
        }


        private readonly EdoContext _context;
    }
}
