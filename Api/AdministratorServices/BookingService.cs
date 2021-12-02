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
            var query = from booking in _context.Bookings
                join agent in _context.Agents on booking.AgentId equals agent.Id
                join agency in _context.Agencies on booking.AgencyId equals agency.Id
                select new BookingSlim
                {
                    Id = booking.Id,
                    ReferenceCode = booking.ReferenceCode,
                    AccommodationName = booking.AccommodationName,
                    AgencyId = booking.AgencyId,
                    AgentId = booking.AgentId,
                    AgencyName = agency.Name,
                    AgentName = $"{agent.FirstName} {agent.LastName}",
                    Created = booking.Created,
                    Currency = booking.Currency,
                    PaymentStatus = booking.PaymentStatus,
                    TotalPrice = booking.TotalPrice,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate
                };

            return expression == null
                ? query
                : query.Where(expression);
        }


        private readonly EdoContext _context;
    }
}
