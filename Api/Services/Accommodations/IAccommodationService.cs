using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;
using AvailabilityRequest = HappyTravel.Edo.Api.Models.Availabilities.AvailabilityRequest;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public interface IAccommodationService
    {
        ValueTask<Result<AccommodationDetails, ProblemDetails>> Get(string accommodationId, string languageCode);

        ValueTask<Result<AvailabilityDetails, ProblemDetails>> GetAvailable(AvailabilityRequest request, string languageCode);

        Task<Result<SingleAccommodationAvailabilityDetails, ProblemDetails>> GetAvailable(string accommodationId, long availabilityId, string languageCode);

        Task<Result<BookingDetails, ProblemDetails>> Book(AccommodationBookingRequest request, string languageCode);

        Task<Result<AccommodationBookingInfo>> GetBooking(int bookingId);

        Task<Result<AccommodationBookingInfo>> GetBooking(string referenceCode);

        Task<Result<List<SlimAccommodationBookingInfo>>> GetCustomerBookings();

        Task<Result<VoidObject, ProblemDetails>> CancelBooking(int bookingId);


        Task<Result<SingleAccommodationAvailabilityDetailsWithDeadline, ProblemDetails>> GetExactAvailability(long availabilityId, Guid agreementId,
            string languageCode);


        Task<Result<List<int>>> GetBookingsForCancellation(DateTime deadlineDate);

        Task<Result<ProcessResult>> CancelBookings(List<int> bookingIds);
    }
}