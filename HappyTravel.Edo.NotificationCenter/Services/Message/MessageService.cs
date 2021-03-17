using System;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.NotificationCenter.Enums;
using HappyTravel.Edo.NotificationCenter.Models;

namespace HappyTravel.Edo.NotificationCenter.Services.Message
{
    public class MessageService : IMessageService
    {
        public MessageService(Hub.SignalRSender signalRSender)
        {
            _signalRSender = signalRSender;
        }
        
        public async Task Add(Request request)
        {
            // TODO: store message in database
            const int messageId = default;
            
            if (request.Protocols.Contains(ProtocolTypes.WebSocket))
                await SendSignalR(request, messageId);

            if (request.Protocols.Contains(ProtocolTypes.Email))
                await SendEmail(request);
        }


        public Task MarkAsRead(int messageId)
        {
            // TODO: find message in storage and mark as read
            return Task.CompletedTask;
        }


        public Task GetMessages()
        {
            // TODO: get messages list from storage
            return Task.CompletedTask;
        }


        private Task SendSignalR(Request request, int messageId)
        {
            return request.MessageType switch
            {
                MessageType.BookingVoucher 
                    => _signalRSender.SendBookingVoucher(request.AgentId, messageId, request.ShortMessage),
                
                MessageType.BookingInvoice 
                    => _signalRSender.SendBookingInvoice(request.AgentId, messageId, request.ShortMessage),
                
                MessageType.DeadlineApproaching 
                    => _signalRSender.SendDeadlineApproaching(request.AgentId, messageId, request.ShortMessage),
                
                MessageType.SuccessfulPaymentReceipt 
                    => _signalRSender.SendSuccessfulPaymentReceipt(request.AgentId, messageId, request.ShortMessage),
                
                MessageType.BookingDuePayment 
                    => _signalRSender.SendBookingBuePayment(request.AgentId, messageId, request.ShortMessage),
                
                MessageType.BookingCancelled 
                    => _signalRSender.SendBookingCancelled(request.AgentId, messageId, request.ShortMessage),
                
                MessageType.BookingFinalized 
                    => _signalRSender.SendBookingFinalized(request.AgentId, messageId, request.ShortMessage),
                
                _ => throw new ArgumentOutOfRangeException()
            };
        }


        private Task SendEmail(Request request)
        {
            // TODO: implement sending e-mails
            return Task.CompletedTask;
        }


        private readonly Hub.SignalRSender _signalRSender;
    }
}