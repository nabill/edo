using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing
{
    public interface IBookingResponseProcessor
    {
        Task ProcessResponse(Booking bookingResponse, UserInfo user, Data.Bookings.BookingChangeReason changeReason);
    }
}