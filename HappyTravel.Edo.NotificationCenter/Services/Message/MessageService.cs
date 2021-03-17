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
            
            if (request.Protocols.Contains(ProtocolTypes.WebSocket))
                await SendSignalR(request);

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


        private Task SendSignalR(Request request)
        {
            return request.MessageType switch
            {
                MessageType.BookingVoucher 
                    => _signalRSender.SendBookingVoucher(request.AgentId, request.ShortMessage),
                
                MessageType.BookingInvoice 
                    => _signalRSender.SendBookingInvoice(request.AgentId, request.ShortMessage),
                
                MessageType.DeadlineApproaching 
                    => _signalRSender.SendDeadlineApproaching(request.AgentId, request.ShortMessage),
                
                MessageType.SuccessfulPaymentReceipt 
                    => _signalRSender.SendSuccessfulPaymentReceipt(request.AgentId, request.ShortMessage),
                
                MessageType.BookingDuePayment 
                    => _signalRSender.SendBookingBuePayment(request.AgentId, request.ShortMessage),
                
                MessageType.BookingCancelled 
                    => _signalRSender.SendBookingCancelled(request.AgentId, request.ShortMessage),
                
                MessageType.BookingFinalized 
                    => _signalRSender.SendBookingFinalized(request.AgentId, request.ShortMessage),
                
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