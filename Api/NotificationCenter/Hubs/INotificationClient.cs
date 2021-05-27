using System.Text.Json;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.NotificationCenter.Hubs
{
    public interface INotificationClient
    {
        Task ReceiveMessage(int messageId, string notificationType, JsonDocument message);
    }
}