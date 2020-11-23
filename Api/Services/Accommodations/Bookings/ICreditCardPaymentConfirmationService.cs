using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface ICreditCardPaymentConfirmationService
    {
        Task<Result<Data.Booking.Booking, ProblemDetails>> Confirm(int bookingId);
    }
}