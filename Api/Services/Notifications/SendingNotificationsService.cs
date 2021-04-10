using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.NotificationCenter.Services.Notification;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.Notifications.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Notifications
{
    public class SendingNotificationsService : ISendingNotificationsService
    {
        public SendingNotificationsService(INotificationService notificationService, INotificationOptionsService notificationOptionsService)
        {
            _notificationService = notificationService;
            _notificationOptionsService = notificationOptionsService;
        }


        public async Task<Result> Send(SlimAgentContext agent, string message, NotificationTypes notificationType, string email = "", string templateId = "")
        {
            return await Send(agent, message, notificationType, new List<string> { email }, templateId);
        }


        public async Task<Result> Send(SlimAgentContext agent, string message, NotificationTypes notificationType, List<string> emails = null, string templateId = "")
        {
            return await _notificationOptionsService.GetNotificationOptions(notificationType, agent)
                .Map(GetSettings)
                .Tap(sendingSettings => AddNotification(ReceiverTypes.AgentApp, agent.AgentId, message, notificationType, sendingSettings));


            Dictionary<ProtocolTypes, ISendingSettings> GetSettings(SlimNotificationOptions notificationOptions)
            {
                var sendingSettings = new Dictionary<ProtocolTypes, ISendingSettings>();

                if ((notificationOptions.EnabledProtocols & ProtocolTypes.WebSocket) == ProtocolTypes.WebSocket)
                    sendingSettings.Add(ProtocolTypes.WebSocket, new WebSocketSettings { });

                if ((notificationOptions.EnabledProtocols & ProtocolTypes.Email) == ProtocolTypes.Email)
                    sendingSettings.Add(ProtocolTypes.Email, new EmailSettings { Emails = emails ?? new(0), TemplateId = templateId });

                return sendingSettings;
            }
        }


        private async Task AddNotification(ReceiverTypes receiver, int userId, string message, NotificationTypes notificationType, Dictionary<ProtocolTypes, ISendingSettings> sendingSettings)
        {
            await _notificationService.Add(new Notification
            {
                Receiver = receiver,
                UserId = userId,
                Message = message,
                Type = notificationType,
                SendingSettings = sendingSettings
            });
        }


        private readonly INotificationService _notificationService;
        private readonly INotificationOptionsService _notificationOptionsService;
    }
}
