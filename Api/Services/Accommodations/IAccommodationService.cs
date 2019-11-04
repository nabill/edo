using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;
using AvailabilityRequest = HappyTravel.Edo.Api.Models.Availabilities.AvailabilityRequest;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public interface IAccommodationService
    {
        ValueTask<Result<RichAccommodationDetails, ProblemDetails>> Get(string accommodationId, string languageCode);
        ValueTask<Result<AvailabilityDetails, ProblemDetails>> GetAvailable(AvailabilityRequest request, string languageCode);
        Task<Result<AccommodationBookingDetails, ProblemDetails>> Book(AccommodationBookingRequest request, string languageCode);
        Task<List<AccommodationBookingInfo>> GetBookings();
        Task<Result<VoidObject, ProblemDetails>> CancelBooking(int bookingId);
        Task<Result<BookingAvailabilityInfo, ProblemDetails>> GetBookingAvailability(int availabilityId, Guid agreementId, string languageCode);

    }
}