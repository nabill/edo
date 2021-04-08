using HappyTravel.Edo.Common.Enums.Notifications;

namespace HappyTravel.Edo.Common.Models.Notifications
{
    public class WebSocketSettings : ISendingSettings
    {
        public NotificationTypes NotificationType { get; init; }
    }
}