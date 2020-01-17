using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public class BookingResponseDataLogService: IBookingResponseDataLogService
    {
        public BookingResponseDataLogService(EdoContext edoContext)
        {
            _edoContext = edoContext;
        }
        
        public async Task<Result> Add(BookingDetails bookingDetails)
        {
            var booking = await _edoContext.Bookings.SingleOrDefaultAsync(i => i.ReferenceCode.Equals(bookingDetails.ReferenceCode));
            
            if (booking == null || booking.Id.Equals(default))
                return Result.Fail<BookingRequestDataEntry>("Failed to add booking response data");
            
            _edoContext.BookingResponseLogs.Add(new BookingResponseDataEntry()
            {
                BookingId = booking.Id,
                CustomerId = booking.CustomerId,
                BookingDetails = bookingDetails
            });
            
            await _edoContext.SaveChangesAsync();
            
            return Result.Ok();
        }

        private readonly EdoContext _edoContext;
    }
}