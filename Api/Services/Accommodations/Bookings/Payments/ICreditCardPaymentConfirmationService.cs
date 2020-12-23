using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments
{
    public interface ICreditCardPaymentConfirmationService
    {
        Task<Result> Confirm(int bookingId);
    }
}