using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing
{
    public interface IBookingNotificationService
    {
        Task NotifyBookingCancelled(AccommodationBookingInfo bookingInfo);

        Task NotifyBookingFinalized(AccommodationBookingInfo bookingInfo);

        Task<Result> NotifyDeadlineApproaching(int bookingId, string email);

        Task<Result> NotifyCreditCardPaymentConfirmed(string referenceCode);
        
        Task NotifyBookingManualCorrectionNeeded(string referenceCode, string agentName, string agencyName, string deadline);
    }
}