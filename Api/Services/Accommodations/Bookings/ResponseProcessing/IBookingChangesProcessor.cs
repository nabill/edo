using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing
{
    public interface IBookingChangesProcessor
    {
        Task<Result> ProcessCancellation(Booking booking, UserInfo user);

        Task ProcessConfirmation(Booking booking, EdoContracts.Accommodations.Booking bookingResponse);

        Task ProcessBookingNotFound(Booking booking, EdoContracts.Accommodations.Booking bookingResponse);

        Task ProcessRejection(Booking booking, UserInfo internalServiceAccount);

        Task<Result> ProcessDiscarding(Booking booking, UserInfo user);
    }
}