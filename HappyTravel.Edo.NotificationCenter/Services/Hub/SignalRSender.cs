using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace HappyTravel.Edo.NotificationCenter.Services.Hub
{
    public class SignalRSender : Hub<INotificationCenter>
    {
        public Task FireNotificationAddedEvent(int agentId, int messageId, string message)
            => Clients.Group(BuildAgentGroupName(agentId)).NotificationAdded(messageId, message);


        public Task Join(string roomName) 
            => Groups.AddToGroupAsync(Context.ConnectionId, roomName);


        public Task Leave(string roomName) 
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);


        private static string BuildAgentGroupName(int agentId) 
            => $"agent-{agentId}";
    }
}