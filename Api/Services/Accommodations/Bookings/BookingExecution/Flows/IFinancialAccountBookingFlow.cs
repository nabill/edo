using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution.Flows
{
    public interface IFinancialAccountBookingFlow
    {
        Task<Result<AccommodationBookingInfo>> BookByAccount(AccommodationBookingRequest bookingRequest, string languageCode);
    }
}