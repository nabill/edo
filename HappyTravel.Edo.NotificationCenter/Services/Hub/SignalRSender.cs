using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace HappyTravel.Edo.NotificationCenter.Services.Hub
{
    public class SignalRSender : Hub<INotificationCenter>
    {
        public Task SendBookingVoucher(int agentId, int messageId, string message)
            => Clients.Group(BuildAgentGroupName(agentId)).BookingVoucher(messageId, message);
        
        
        public Task SendBookingInvoice(int agentId, int messageId, string message)
            => Clients.Group(BuildAgentGroupName(agentId)).BookingInvoice(messageId, message);
        
        
        public Task SendDeadlineApproaching(int agentId, int messageId, string message)
            => Clients.Group(BuildAgentGroupName(agentId)).DeadlineApproaching(messageId, message);
        
        
        public Task SendSuccessfulPaymentReceipt(int agentId, int messageId, string message)
            => Clients.Group(BuildAgentGroupName(agentId)).SuccessfulPaymentReceipt(messageId, message);
        
        
        public Task SendBookingBuePayment(int agentId, int messageId, string message)
            => Clients.Group(BuildAgentGroupName(agentId)).BookingBuePayment(messageId, message);
        
        
        public Task SendBookingCancelled(int agentId, int messageId, string message)
            => Clients.Group(BuildAgentGroupName(agentId)).BookingCancelled(messageId, message);
        
        
        public Task SendBookingFinalized(int agentId, int messageId, string message)
            => Clients.Group(BuildAgentGroupName(agentId)).BookingFinalized(messageId, message);


        public Task Join(string roomName) 
            => Groups.AddToGroupAsync(Context.ConnectionId, roomName);


        public Task Leave(string roomName) 
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);


        private static string BuildAgentGroupName(int agentId) 
            => $"agent-{agentId}";
    }
}