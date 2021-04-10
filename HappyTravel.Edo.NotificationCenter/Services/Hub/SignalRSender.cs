using System.Threading.Tasks;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.AspNetCore.SignalR;

namespace HappyTravel.Edo.NotificationCenter.Services.Hub
{
    public class SignalRSender : Hub<INotificationCenter>
    {
        public Task FireNotificationAddedEvent(ReceiverTypes receiver, int userId, int messageId, string message)
            => Clients.Group(BuildUserGroupName(receiver, userId)).NotificationAdded(messageId, message);


        public Task Join(string roomName) 
            => Groups.AddToGroupAsync(Context.ConnectionId, roomName);


        public Task Leave(string roomName) 
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);


        private static string BuildUserGroupName(ReceiverTypes receiver, int userId)
        {
            switch (receiver)
            {
                case ReceiverTypes.AgentApp:
                    return $"agent-{userId}";
                case ReceiverTypes.AdminPanel:
                    return $"admin-{userId}";
            }
            return $"unknown-{userId}";
        }
    }
}