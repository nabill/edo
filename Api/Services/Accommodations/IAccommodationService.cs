using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.AspNetCore.Mvc;
using AvailabilityRequest = HappyTravel.Edo.Api.Models.Availabilities.AvailabilityRequest;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public interface IAccommodationService
    {
        ValueTask<Result<AccommodationDetails, ProblemDetails>> Get(string accommodationId, string languageCode);

        ValueTask<Result<AvailabilityDetails, ProblemDetails>> GetAvailable(AvailabilityRequest request, string languageCode);

        Task<Result<SingleAccommodationAvailabilityDetails, ProblemDetails>> GetAvailable(string accommodationId, long availabilityId, string languageCode);

        Task<Result<BookingDetails, ProblemDetails>> SendBookingRequest(AccommodationBookingRequest request, string languageCode);

        Task<Result<AccommodationBookingInfo>> GetBooking(int bookingId);

        Task<Result<AccommodationBookingInfo>> GetBooking(string referenceCode);

        Task<Result<List<SlimAccommodationBookingInfo>>> GetCustomerBookings();

        Task<Result<VoidObject, ProblemDetails>> SendCancellationBookingRequest(int bookingId);

        Task<Result<SingleAccommodationAvailabilityDetailsWithDeadline, ProblemDetails>> GetExactAvailability(long availabilityId, Guid agreementId,
            string languageCode);

        Task<Result<List<int>>> GetBookingsForCancellation(DateTime deadlineDate);

        Task<Result<ProcessResult>> CancelBookings(List<int> bookingIds);

        Task<Result> ProcessBookingResponse(BookingDetails bookingResponse, Booking currentBooking = null);

        Task<Result<string, ProblemDetails>> CreateBookingForPayment(PaymentMethods paymentMethod, PaymentRequest request);

        Task<Result<string, ProblemDetails>> CreateBookingForPayment(PaymentMethods paymentMethod, AccountPaymentRequest request);
    }
}