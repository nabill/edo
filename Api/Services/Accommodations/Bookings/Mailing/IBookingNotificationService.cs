using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing
{
    public interface IBookingNotificationService
    {
        Task NotifyBookingCancelled(AccommodationBookingInfo bookingInfo, SlimAgentContext agent);

        Task NotifyBookingFinalized(AccommodationBookingInfo bookingInfo, SlimAgentContext agent);

        Task<Result> NotifyDeadlineApproaching(int bookingId, string email);

        Task<Result> NotifyOfflineDeadlineApproaching(int bookingId, OfflineDeadlineNotifications notificationType,
            OfflineDeadlineNotifications? notificationForBooking = null);

        Task NotifyBookingManualCorrectionNeeded(string referenceCode, string agentName, string agencyName, string deadline);

        Task NotifyAdminsStatusChanged(AccommodationBookingInfo bookingInfo);
    }
}