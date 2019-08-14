using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Bookings
{
    public interface IBookingService
    {
        Task<Result<AccommodationBookingDetails, ProblemDetails>> BookAccommodation(AccommodationBookingRequest request, string languageCode);
    }
}