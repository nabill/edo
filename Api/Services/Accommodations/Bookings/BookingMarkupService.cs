using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingMarkupService : IBookingMarkupService
    {
        public BookingMarkupService(EdoContext context) => _context = context;

        public async Task Create(IEnumerable<BookingMarkup> appliedMarkups)
        {
            _context.BookingMarkups.AddRange(appliedMarkups);
            await _context.SaveChangesAsync();
        }


        private readonly EdoContext _context;
    }
}