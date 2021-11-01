using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
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


        public async Task<Result<Booking>> Get(BookingIdentifier bookingIdentifier, AgentContext agent)
        {
            return await Validate()
                .Bind(GetBooking);
            

            Result Validate()
            {
                var validator = new InlineValidator<BookingIdentifier>();
                validator.RuleFor(i => i.ClientReferenceCode).NotEmpty().When(i => string.IsNullOrWhiteSpace(i.SupplierReferenceCode));
                validator.RuleFor(i => i.SupplierReferenceCode).NotEmpty().When(i => string.IsNullOrWhiteSpace(i.ClientReferenceCode));

                var result = validator.Validate(bookingIdentifier);
                return result.IsValid
                    ? Result.Success()
                    : Result.Failure(result.ToString("; "));
            }


            async Task<Result<Booking>> GetBooking()
            {
                var query = _context.Bookings
                    .Where(b => b.AgentId == agent.AgentId && b.AgentId == agent.AgencyId);

                if (!string.IsNullOrWhiteSpace(bookingIdentifier.ClientReferenceCode))
                    query = query.Where(b => b.ClientReferenceCode == bookingIdentifier.ClientReferenceCode);

                if (!string.IsNullOrWhiteSpace(bookingIdentifier.SupplierReferenceCode))
                    query = query.Where(b => b.ReferenceCode == bookingIdentifier.SupplierReferenceCode);

                var booking = await query.SingleOrDefaultAsync();
                return booking?.FromEdoModels() ?? Result.Failure<Booking>("Booking not found");
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