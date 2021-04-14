using HappyTravel.Edo.Notifications.Enums;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.NotificationCenter.Hubs
{
    public class NotificationHub : Hub<INotificationClient>
    {
        public static async Task SendPrivateMessage(IHubContext<NotificationHub, INotificationClient> hub, ReceiverTypes receiver, int userId, int messageId, string message)
        {
            await hub.Clients
                .User(BuildUserId(receiver, userId))
                .ReceiveMessage(messageId, message);
        }


        private static string BuildUserId(ReceiverTypes receiver, int userId)
        {
            return receiver switch
            {
                ReceiverTypes.AgentApp => $"agent-{userId}",
                ReceiverTypes.AdminPanel => $"admin-{userId}",
                _ => $"unknown-{userId}",
            };
        }
    }
}