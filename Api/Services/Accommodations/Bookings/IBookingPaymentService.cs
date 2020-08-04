using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface IBookingPaymentService : IPaymentsService
    {
        Task<Result<string>> CaptureMoney(Booking booking, UserInfo user);

        Task<Result> VoidOrRefundMoney(Booking booking, UserInfo user);

        Task<Result> CompleteOffline(int bookingId, Administrator administratorContext);
    }
}