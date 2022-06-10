using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.DirectApi.Enum;
using Microsoft.EntityFrameworkCore;
using HappyTravel.Edo.DirectApi.Models.Booking;

namespace HappyTravel.Edo.DirectApi.Services.Bookings
{
    public class BookingInfoService
    {
        public BookingInfoService(EdoContext context, IAgentContextService agentContext)
        {
            _context = context;
            _agentContext = agentContext;
        }


        public async Task<Result<Booking>> GetConvertedBooking(string clientReferenceCode)
        {
            var (_, isFailure, booking, error) = await Get(clientReferenceCode);
            return isFailure
                ? Result.Failure<Booking>(error)
                : booking.FromEdoModels();
        }


        public async Task<Result<Edo.Data.Bookings.Booking>> Get(string clientReferenceCode)
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
                var agent = await _agentContext.GetAgent();
                var booking = await _context.Bookings
                    .SingleOrDefaultAsync(b => b.AgentId == agent.AgentId &&
                        b.AgencyId == agent.AgencyId &&
                        b.ClientReferenceCode == clientReferenceCode);
                
                return booking ?? Result.Failure<Edo.Data.Bookings.Booking>("Booking not found");
            }
        }


        public async Task<List<SlimBooking>> Get(BookingsListFilter filter)
        {
            var agent = await _agentContext.GetAgent();
            var query = from booking in _context.Bookings
                where booking.AgentId == agent.AgentId &&
                    booking.AgencyId == agent.AgencyId
                select booking;

            if (filter.CreatedFrom is not null)
                query = query.Where(b => b.Created >= filter.CreatedFrom);

            if (filter.CreatedTo is not null)
                query = query.Where(b => b.Created <= filter.CreatedTo);
            
            if (filter.CheckInFrom is not null)
                query = query.Where(b => b.CheckInDate >= filter.CheckInFrom);

            if (filter.CheckInTo is not null)
                query = query.Where(b => b.CheckInDate <= filter.CheckInTo);

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
        private readonly IAgentContextService _agentContext;
    }
}