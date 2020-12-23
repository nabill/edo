using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments
{
    public interface IBookingOfflinePaymentService
    {
        Task<Result> CompleteOffline(int bookingId, Administrator administratorContext);
    }
}