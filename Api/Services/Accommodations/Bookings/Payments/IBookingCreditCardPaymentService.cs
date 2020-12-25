using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments
{
    public interface IBookingCreditCardPaymentService
    {
        Task<Result<string>> Capture(Booking booking, UserInfo toUserInfo);

        Task<Result> Void(Booking booking, UserInfo user);

        Task<Result> Refund(Booking booking, UserInfo user);
    }
}