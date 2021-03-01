using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public interface IBookingRecordManager
    {
        Task<Result<Booking>> Get(string referenceCode);
        
        Task<Result<Booking>> Get(int id);

        Task SetConfirmationDate(Booking booking);
        
        Task UpdateBookingFromDetails(EdoContracts.Accommodations.Booking bookingDetails, Booking booking);
 
        Task<string> Register(AccommodationBookingRequest bookingRequest, BookingAvailabilityInfo bookingAvailability, PaymentMethods paymentMethod, AgentContext agentContext, string languageCode);

        Task SetStatus(string referenceCode, BookingStatuses status);
    }
}