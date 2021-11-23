using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.NotificationCenter.Models;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing
{
    public interface IBookingNotificationService
    {
        Task NotifyBookingCancelled(AccommodationBookingInfo bookingInfo, SlimAgentContext agent);

        Task NotifyBookingFinalized(AccommodationBookingInfo bookingInfo, SlimAgentContext agent);

        Task<Result> NotifyDeadlineApproaching(int bookingId, string email);

        Task<Result> NotifyCreditCardPaymentConfirmed(string referenceCode);
        
        Task NotifyBookingManualCorrectionNeeded(string referenceCode, string agentName, string agencyName, string deadline);

        Task NotifyAdminsStatusChanged(BookingStatusChangeInfo message);
    }
}