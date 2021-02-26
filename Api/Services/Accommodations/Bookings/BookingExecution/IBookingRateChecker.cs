using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution
{
    public interface IBookingRateChecker
    {
        Task<Result> Check(AccommodationBookingRequest bookingRequest, BookingAvailabilityInfo availabilityInfo, PaymentMethods paymentMethod, AgentContext agent);
    }
}