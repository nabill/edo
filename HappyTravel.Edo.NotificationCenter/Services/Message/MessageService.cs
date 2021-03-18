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
        
        public async Task Add(NotificationInfo request)
        {
            // TODO: store message in database
            const int messageId = default;

            if (request.Protocols.Contains(ProtocolTypes.WebSocket))
                await _signalRSender.SendNotificationAdded(request.AgentId, messageId, request.Message);

            if (request.Protocols.Contains(ProtocolTypes.Email) && request.EmailSettings.HasValue)
                await SendEmail(request.EmailSettings.Value);
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


        private Task SendEmail(EmailSettings settings)
        {
            // TODO: implement sending e-mails
            return Task.CompletedTask;
        }


        private readonly Hub.SignalRSender _signalRSender;
    }
}