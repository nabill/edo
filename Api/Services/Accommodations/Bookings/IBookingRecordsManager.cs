using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface IBookingRecordsManager
    {
        Task<Result<AccommodationBookingInfo>> GetAgentBookingInfo(int bookingId, string languageCode);
        
        Task<Result<AccommodationBookingInfo>> GetAgentBookingInfo(string referenceCode, string languageCode);
        
        Task<Result<Booking>> Get(string referenceCode);
        
        Task<Result<Booking>> Get(int id);

        Task<Result<Booking>> Get(int bookingId, int agentId);
        
        Task<Result<List<SlimAccommodationBookingInfo>>> GetAgentBookingsInfo();
        
        Task Confirm(BookingDetails bookingDetails, Booking booking);
        
        Task ConfirmBookingCancellation(Booking booking);
        
        Task UpdateBookingDetails(BookingDetails bookingDetails, Booking booking);
 
        Task<string> Register(AccommodationBookingRequest bookingRequest, BookingAvailabilityInfo bookingAvailability, string languageCode);

        Task<Result<Booking>> GetAgentsBooking(string referenceCode);
    }
}