using System.Threading.Tasks;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface IBookingAuditLogService
    {
        Task Add(BookingDetails newBookingDetails, Data.Booking.Booking currentBooking = null);
    }
}