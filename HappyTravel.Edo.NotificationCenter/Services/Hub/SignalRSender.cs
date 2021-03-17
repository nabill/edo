using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace HappyTravel.Edo.NotificationCenter.Services.Hub
{
    public class SignalRSender : Hub<INotificationCenter>
    {
        public Task SendBookingVoucher(int agentId, string message)
            => Clients.Group(BuildAgentGroupName(agentId)).BookingVoucher(message);
        
        
        public Task SendBookingInvoice(int agentId, string message)
            => Clients.Group(BuildAgentGroupName(agentId)).BookingInvoice(message);
        
        
        public Task SendDeadlineApproaching(int agentId, string message)
            => Clients.Group(BuildAgentGroupName(agentId)).DeadlineApproaching(message);
        
        
        public Task SendSuccessfulPaymentReceipt(int agentId, string message)
            => Clients.Group(BuildAgentGroupName(agentId)).SuccessfulPaymentReceipt(message);
        
        
        public Task SendBookingBuePayment(int agentId, string message)
            => Clients.Group(BuildAgentGroupName(agentId)).BookingBuePayment(message);
        
        
        public Task SendBookingCancelled(int agentId, string message)
            => Clients.Group(BuildAgentGroupName(agentId)).BookingCancelled(message);
        
        
        public Task SendBookingFinalized(int agentId, string message)
            => Clients.Group(BuildAgentGroupName(agentId)).BookingFinalized(message);


        public Task Join(string roomName) 
            => Groups.AddToGroupAsync(Context.ConnectionId, roomName);


        public Task Leave(string roomName) 
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);


        private static string BuildAgentGroupName(int agentId) 
            => $"agent-{agentId}";
    }
}