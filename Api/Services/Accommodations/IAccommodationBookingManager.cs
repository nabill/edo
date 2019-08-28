using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Bookings;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public interface IAccommodationBookingManager
    {
        Task<Result<AccommodationBookingDetails, ProblemDetails>> Book(AccommodationBookingRequest bookingRequest,
            BookingAvailabilityInfo availabilityInfo, string languageCode);

        Task<List<AccommodationBookingInfo>> GetBookings();
        Task<Result<VoidObject, ProblemDetails>> Cancel(int bookingId);
    }
}