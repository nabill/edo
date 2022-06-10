using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution.Flows
{
    public interface IBankCreditCardBookingFlow
    {
        Task<Result<string>> Register(AccommodationBookingRequest bookingRequest, string languageCode);

        Task<Result<AccommodationBookingInfo>> Finalize(string referenceCode, string languageCode);
    }
}