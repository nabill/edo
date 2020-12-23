using System.Threading.Tasks;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing
{
    public interface IBookingAuditLogService
    {
        Task Add(Booking newBookingDetails, Data.Bookings.Booking currentBooking = null);
    }
}