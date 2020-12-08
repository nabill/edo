using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Data.Booking;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface IBookingMarkupService
    {
        Task Create(IEnumerable<BookingMarkup> appliedMarkups);
    }
}