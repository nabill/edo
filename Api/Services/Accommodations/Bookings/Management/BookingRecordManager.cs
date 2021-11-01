using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;
using Booking = HappyTravel.Edo.Data.Bookings.Booking;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public class BookingRecordManager : IBookingRecordManager
    {
        public BookingRecordManager(EdoContext context)
        {
            _context = context;
        }


        public Task<Result<Booking>> Get(string referenceCode)
        {
            return Get(booking => booking.ReferenceCode == referenceCode);
        }
        

        public Task<Result<Booking>> Get(int bookingId)
        {
            return Get(booking => booking.Id == bookingId);
        }


        private async Task<Result<Booking>> Get(Expression<Func<Booking, bool>> filterExpression)
        {
            var booking = await _context.Bookings
                .Where(filterExpression)
                .SingleOrDefaultAsync();

            return booking == default
                ? Result.Failure<Booking>("Could not get booking data")
                : Result.Success(booking);
        }


        private readonly EdoContext _context;
    }
}