using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface IBookingManager
    {
        Task<BookingDetails> Finalize(
            Booking booking,
            BookingDetails bookingDetails);
        
        Task<Result<AccommodationBookingInfo>> GetCustomerBookingInfo(int bookingId);
        
        Task<Result<AccommodationBookingInfo>> GetCustomerBookingInfo(string referenceCode);
        
        Task<Result<Booking>> Get(string referenceCode);
        
        Task<Result<Booking>> Get(int id);
        
        Task<Result<List<SlimAccommodationBookingInfo>>> GetCustomerBookingsInfo();
        
        Task<Result> ConfirmBooking(BookingDetails bookingDetails, Booking booking);
        
        Task<Result> ConfirmBookingCancellation(BookingDetails bookingDetails, Booking booking);
        
        Task<Result> UpdateBookingDetails(BookingDetails bookingDetails, Booking booking);
 
        Task<Result<string>> Register(AccommodationBookingRequest bookingRequest, BookingAvailabilityInfo bookingAvailability);

        Task<Result<Booking>> GetCustomersBooking(string referenceCode);
    }
}