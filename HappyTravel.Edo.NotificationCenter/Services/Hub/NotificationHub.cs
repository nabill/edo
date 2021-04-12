using System.Threading.Tasks;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.AspNetCore.SignalR;

namespace HappyTravel.Edo.NotificationCenter.Services.Hub
{
    public class NotificationHub : Microsoft.AspNetCore.SignalR.Hub<INotificationClient>
    {
        //public Task FireNotificationAddedEvent(ReceiverTypes receiver, int userId, int messageId, string message)
        //    => Clients.Group(BuildUserGroupName(receiver, userId)).ReceiveMessage(messageId, message);

        public async Task SendPrivateMessage(ReceiverTypes receiver, int userId, int messageId, string message)
            => await Clients.User(BuildUserName(receiver, userId)).ReceiveMessage(messageId, message);
        

        //public Task Join(string roomName) 
        //    => Groups.AddToGroupAsync(Context.ConnectionId, roomName);


        //public Task Leave(string roomName) 
        //   => Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);


        private static string BuildUserName(ReceiverTypes receiver, int userId)
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