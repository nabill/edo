using HappyTravel.Edo.Notifications.Enums;
using System.Text.Json;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.NotificationCenter.Hubs
{
    public interface INotificationClient
    {
        Task ReceiveMessage(int messageId, NotificationTypes notificationType, JsonDocument message);
    }
}