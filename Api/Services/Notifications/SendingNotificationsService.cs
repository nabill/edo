using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.Notifications.Models;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Notifications
{
    public class SendingNotificationsService : ISendingNotificationsService
    {
        public SendingNotificationsService(INotificationService notificationService, 
            INotificationOptionsService notificationOptionsService,
            IAgentContextService agentContextService)
        {
            _notificationService = notificationService;
            _notificationOptionsService = notificationOptionsService;
            _agentContextService = agentContextService;
        }


        public async Task<Result> Send(ApiCaller apiCaller, JsonDocument message, NotificationTypes notificationType, string email = "", string templateId = "")
        {
            if (apiCaller.Type != Common.Enums.ApiCallerTypes.Agent)    // TODO: The implementation of sending messages for the admin will be in a separate task.
                return Result.Success();

            var agent = await _agentContextService.GetAgent();

            return await Send(new SlimAgentContext(agent.AgentId, agent.AgencyId), message, notificationType, new List<string> { email }, templateId);
        }


        public async Task<Result> Send(SlimAgentContext agent, JsonDocument message, NotificationTypes notificationType, string email = "", string templateId = "")
        {
            return await Send(agent, message, notificationType, new List<string> { email }, templateId);
        }


        public async Task<Result> Send(SlimAgentContext agent, JsonDocument message, NotificationTypes notificationType, List<string> emails = null, string templateId = "")
        {
            return await _notificationOptionsService.GetNotificationOptions(notificationType, agent)
                .Map(GetSettings)
                .Tap(sendingSettings => AddNotification(ReceiverTypes.AgentApp, agent, message, notificationType, sendingSettings));


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


        private async Task AddNotification(ReceiverTypes receiver, SlimAgentContext agent, JsonDocument message, NotificationTypes notificationType, Dictionary<ProtocolTypes, ISendingSettings> sendingSettings)
        {
            await _notificationService.Add(new Notification
            {
                Receiver = receiver,
                UserId = agent.AgentId,
                AgencyId = agent.AgencyId,
                Message = message,
                Type = notificationType,
                SendingSettings = sendingSettings
            });
        }


        private readonly INotificationService _notificationService;
        private readonly INotificationOptionsService _notificationOptionsService;
        private readonly IAgentContextService _agentContextService;
    }
}
