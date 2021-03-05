using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public interface IBookingRecordManager
    {
        Task<Result<Booking>> Get(string referenceCode);
        
        Task<Result<Booking>> Get(int id);
 
        // TODO: Move registration to a separate service
        Task<string> Register(AccommodationBookingRequest bookingRequest, BookingAvailabilityInfo bookingAvailability, PaymentMethods paymentMethod, AgentContext agentContext, string languageCode);
    }
}