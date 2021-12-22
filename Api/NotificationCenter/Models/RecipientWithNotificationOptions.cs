using HappyTravel.Edo.Notifications.Models;

namespace HappyTravel.Edo.Api.NotificationCenter.Models
{
    public class RecipientWithNotificationOptions
    {
        public int RecipientId { get; init; }
        public string Email { get; init; }
        public SlimNotificationOptions NotificationOptions { get; init; }
    }
}
