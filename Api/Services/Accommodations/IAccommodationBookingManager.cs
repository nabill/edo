using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public interface IAccommodationBookingManager
    {
        Task<Result<BookingDetails, ProblemDetails>> SendBookingRequest(AccommodationBookingRequest bookingRequest, BookingAvailabilityInfo availabilityInfo, string languageCode);
        Task<Result<AccommodationBookingInfo>> GetCustomerBooking(int bookingId);
        Task<Result<AccommodationBookingInfo>> GetCustomerBooking(string referenceCode);
        Task<Result<Booking>> GetRaw(string referenceCode);
        Task<Result<Booking>> GetRaw(int id);
        Task<Result<Booking>> GetRawCustomerBooking(int bookingId);
        Task<Result<List<SlimAccommodationBookingInfo>>> GetAll();
        Task<Result<Booking, ProblemDetails>> SendCancelBookingRequest(int bookingId);
        Task<Result<Booking>> ChangeBookingToCancelled(Booking bookingToCancel);
        Task<Result<Booking>> UpdateBookingDetails(Booking booking, BookingDetails bookingDetails);
    }
}
