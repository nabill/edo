using HappyTravel.Edo.Notifications.Enums;

namespace HappyTravel.Edo.Notifications.Models
{
    public class WebSocketSettings : ISendingSettings
    {
        public NotificationTypes NotificationType { get; init; }
    }
}