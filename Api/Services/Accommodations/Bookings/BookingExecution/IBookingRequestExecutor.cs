using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution
{
    public interface IBookingRequestExecutor
    {
        Task<Result<Booking>> Execute(Data.Bookings.Booking booking, string languageCode);
    }
}