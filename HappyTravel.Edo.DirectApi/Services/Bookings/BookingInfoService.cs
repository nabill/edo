using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.DirectApi.Enum;
using Microsoft.EntityFrameworkCore;
using HappyTravel.Edo.DirectApi.Models;
using HappyTravel.Edo.DirectApi.Models.Booking;

namespace HappyTravel.Edo.DirectApi.Services.Bookings
{
    public class BookingInfoService
    {
        public BookingInfoService(EdoContext context)
        {
            _context = context;
        }


        public async Task<Result<Edo.Data.Bookings.Booking>> Get(string clientReferenceCode, AgentContext agent)
        {
            return await Validate()
                .Bind(GetBooking);
            

            Result Validate()
            {
                return string.IsNullOrWhiteSpace(clientReferenceCode)
                    ? Result.Failure("Client reference code code must be set")
                    : Result.Success();
            }


            async Task<Result<Edo.Data.Bookings.Booking>> GetBooking()
            {
                var booking = await _context.Bookings
                    .SingleOrDefaultAsync(b => b.AgentId == agent.AgentId &&
                        b.AgencyId == agent.AgencyId &&
                        b.ClientReferenceCode == clientReferenceCode);
                
                return booking ?? Result.Failure<Edo.Data.Bookings.Booking>("Booking not found");
            }
        }


        public async Task<List<BookingSlim>> Get(BookingsListFilter filter, AgentContext agent)
        {
            var query = from booking in _context.Bookings
                where booking.AgentId == agent.AgentId &&
                    booking.AgencyId == agent.AgencyId
                select booking;

            if (filter.CreatedFrom is not null)
                query = query.Where(b => b.Created >= filter.CreatedFrom);

            if (filter.CreatedTo is not null)
                query = query.Where(b => b.Created <= filter.CreatedTo);
            
            if (filter.CheckinFrom is not null)
                query = query.Where(b => b.CheckInDate >= filter.CheckinFrom);

            if (filter.CheckinTo is not null)
                query = query.Where(b => b.CheckInDate <= filter.CheckinTo);

            query = filter.OrderBy switch
            {
                BookingListOrderTypes.Created => query.OrderBy(b => b.Created),
                BookingListOrderTypes.Checkin => query.OrderBy(b => b.CheckInDate),
                BookingListOrderTypes.Deadline => query.OrderBy(b => b.DeadlineDate),
                _ => query
            };

            var bookings = await query.ToListAsync();
            return bookings.SlimFromEdoModels();
        }


        private readonly EdoContext _context;
    }
}