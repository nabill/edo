using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Users;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface IBookingChangesProcessor
    {
        Task<Result> ProcessCancellation(Data.Booking.Booking booking, UserInfo user);

        Task ProcessConfirmation(Data.Booking.Booking booking, EdoContracts.Accommodations.Booking bookingResponse);

        Task ProcessBookingNotFound(Data.Booking.Booking booking, EdoContracts.Accommodations.Booking bookingResponse);
    }
}