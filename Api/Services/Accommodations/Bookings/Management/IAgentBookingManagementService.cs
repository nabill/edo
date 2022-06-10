using System.Threading.Tasks;
using Api.Models.Bookings;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public interface IAgentBookingManagementService
    {
        Task<Result> Cancel(int bookingId);

        Task<Result> Cancel(string referenceCode);

        Task<Result> RefreshStatus(int bookingId);

        Task<Result<AccommodationBookingInfo>> RecalculatePrice(string referenceCode, BookingRecalculatePriceRequest request, string languageCode);
    }
}