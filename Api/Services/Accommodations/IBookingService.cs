using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.DataProviders;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public interface IBookingService
    {
        Task<Result<string, ProblemDetails>> Register(AccommodationBookingRequest bookingRequest);

        Task<Result<BookingDetails, ProblemDetails>> Finalize(string referenceCode, string languageCode);
        
        Task<Result> ProcessResponse(BookingDetails bookingResponse, Booking booking = null);

        Task<Result<AccommodationBookingInfo>> Get(int bookingId);

        Task<Result<AccommodationBookingInfo>> Get(string referenceCode);

        Task<Result<List<SlimAccommodationBookingInfo>>> Get();

        Task<Result<VoidObject, ProblemDetails>> Cancel(int bookingId);

        
    }
}