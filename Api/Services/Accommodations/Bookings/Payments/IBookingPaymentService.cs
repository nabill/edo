using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments
{
    public interface IBookingPaymentService 
    {
        Task<Result> VoidOrRefund(Booking booking, UserInfo user);

        Task<Result> CompleteOffline(int bookingId, Administrator administratorContext);
    }
}