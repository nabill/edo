using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Booking;

namespace HappyTravel.Edo.Api.Services.Mailing
{
    public interface IBookingMailingService
    {
        Task<Result> SendVoucher(int bookingId, string email);
        Task<Result> SendInvoice(int bookingId, string email);
        Task<Result> NotifyBookingCancelled(string referenceCode, string email, string customerName);
    }
}
