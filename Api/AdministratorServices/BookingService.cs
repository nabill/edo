using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class BookingService : IBookingService
    {
        public BookingService(EdoContext context)
        {
            _context = context;
        }


        public async Task<Result<List<Booking>>> GetAgencyBookings(int agencyId)
        {
            var agency = await _context.Agencies.SingleOrDefaultAsync(agency => agency.Id == agencyId);
            if (agency is null)
                return Result.Failure<List<Booking>>($"Agency with ID {agencyId} not found");

            return await _context.Bookings.Where(booking => booking.AgencyId == agencyId).ToListAsync();
        }


        public async Task<Result<List<Booking>>> GetCounterpartyBookings(int counterpartyId)
        {
            var counterparty = await _context.Counterparties.SingleOrDefaultAsync(counterparty => counterparty.Id == counterpartyId);
            if (counterparty is null)
                return Result.Failure<List<Booking>>($"Counterparty with ID {counterpartyId} not found");

            return await _context.Bookings.Where(booking => booking.CounterpartyId == counterpartyId).ToListAsync();
        }


        public async Task<Result<List<Booking>>> GetAgentBookings(int agentId)
        {
            var agent = await _context.Agents.SingleOrDefaultAsync(agent => agent.Id == agentId);
            if (agent is null)
                return Result.Failure<List<Booking>>($"Agent with ID {agentId} not found");

            return await _context.Bookings
                .Where(booking => booking.AgentId == agentId)
                .OrderByDescending(booking => booking.CheckInDate)
                .ToListAsync();
        }


        private readonly EdoContext _context;
    }
}
