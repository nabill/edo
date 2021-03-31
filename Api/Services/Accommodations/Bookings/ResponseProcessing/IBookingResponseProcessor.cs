using System.Threading.Tasks;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing
{
    public interface IBookingResponseProcessor
    {
        Task ProcessResponse(Booking bookingResponse, Data.Bookings.BookingChangeReason changeReason);
    }
}