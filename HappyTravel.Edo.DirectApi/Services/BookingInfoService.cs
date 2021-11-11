using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.DirectApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.DirectApi.Services
{
    public class BookingInfoService
    {
        public BookingInfoService(EdoContext context)
        {
            _context = context;
        }


        public async Task<Result<Edo.Data.Bookings.Booking>> Get(string? referenceCode, string? supplierReferenceCode, AgentContext agent)
        {
            return await Validate()
                .Bind(GetBooking);
            

            Result Validate()
            {
                return string.IsNullOrWhiteSpace(referenceCode) && string.IsNullOrWhiteSpace(supplierReferenceCode)
                    ? Result.Failure("Reference code or supplier reference code must be set")
                    : Result.Success();
            }


            async Task<Result<Edo.Data.Bookings.Booking>> GetBooking()
            {
                var query = _context.Bookings
                    .Where(b => b.AgentId == agent.AgentId && b.AgencyId == agent.AgencyId);

                if (!string.IsNullOrWhiteSpace(referenceCode))
                    query = query.Where(b => b.ClientReferenceCode == referenceCode);

                if (!string.IsNullOrWhiteSpace(supplierReferenceCode))
                    query = query.Where(b => b.ReferenceCode == supplierReferenceCode);

                var booking = await query.SingleOrDefaultAsync();
                return booking ?? Result.Failure<Edo.Data.Bookings.Booking>("Booking not found");
            }
        }


        public async Task<List<Booking>> Get(DateTime fromDateTime, DateTime toDateTime, AgentContext agent)
        {
            var query = from booking in _context.Bookings
                where booking.AgentId == agent.AgentId &&
                    booking.AgencyId == agent.AgencyId &&
                    booking.Created >= fromDateTime &&
                    booking.Created <= toDateTime
                select booking;

            var bookings = await query.ToListAsync();
            return bookings.FromEdoModels();
        }


        private readonly EdoContext _context;
    }
}