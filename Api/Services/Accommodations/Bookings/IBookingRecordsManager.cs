using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;


namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface IBookingRecordsManager
    {
        Task<Result<AccommodationBookingInfo>> GetAgentAccommodationBookingInfo(int bookingId, AgentContext agentContext, string languageCode);
        
        Task<Result<AccommodationBookingInfo>> GetAgentAccommodationBookingInfo(string referenceCode, AgentContext agentContext, string languageCode);
        
        Task<Result<Data.Booking.Booking>> Get(string referenceCode);
        
        Task<Result<Data.Booking.Booking>> Get(int id);

        Task<Result<Data.Booking.Booking>> Get(int bookingId, int agentId);
        
        Task<Result<List<SlimAccommodationBookingInfo>>> GetAgentBookingsInfo(AgentContext agentContext);
        
        Task Confirm(HappyTravel.EdoContracts.Accommodations.Booking bookingDetails, Data.Booking.Booking booking);
        
        Task ConfirmBookingCancellation(Edo.Data.Booking.Booking booking);
        
        Task UpdateBookingDetails(HappyTravel.EdoContracts.Accommodations.Booking bookingDetails, Data.Booking.Booking booking);
 
        Task<string> Register(AccommodationBookingRequest bookingRequest, BookingAvailabilityInfo bookingAvailability, AgentContext agentContext, string languageCode);

        Task<Result<Edo.Data.Booking.Booking>> GetAgentsBooking(string referenceCode, AgentContext agentContext);
    }
}