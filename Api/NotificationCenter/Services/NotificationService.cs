using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.Notifications.Models;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.NotificationCenter.Services
{
    public class NotificationService : INotificationService
    {
        public NotificationService(IInternalNotificationService internalNotificationService,
            INotificationOptionsService notificationOptionsService,
            IAgentContextService agentContextService)
        {
            _internalNotificationService = internalNotificationService;
            _notificationOptionsService = notificationOptionsService;
            _agentContextService = agentContextService;
        }


        public async Task<Result> Send(ApiCaller apiCaller, JsonDocument message, NotificationTypes notificationType, string email = "", string templateId = "")
        {
            return await Send(apiCaller, message, notificationType, new List<string> { email }, templateId);
        }


        public async Task<Result> Send(ApiCaller apiCaller, JsonDocument message, NotificationTypes notificationType, List<string> emails = null, string templateId = "")
        {
            if (apiCaller.Type == ApiCallerTypes.Agent)
            {
                var agent = await _agentContextService.GetAgent();

                return await Send(new SlimAgentContext(agent.AgentId, agent.AgencyId), message, notificationType, emails, templateId);
            }
            else if (apiCaller.Type == ApiCallerTypes.Admin)
                return await Send(apiCaller.Id, message, notificationType, emails, templateId);
            else
                return Result.Success();
        }


        public async Task<Result> Send(ApiCaller apiCaller, DataWithCompanyInfo messageData, NotificationTypes notificationType, string email, string templateId)
        {
            return await Send(apiCaller, messageData, notificationType, new List<string> { email }, templateId);
        }


        public async Task<Result> Send(ApiCaller apiCaller, DataWithCompanyInfo messageData, NotificationTypes notificationType, List<string> emails, string templateId)
        {
            if (apiCaller.Type == ApiCallerTypes.Agent)
            {
                var agent = await _agentContextService.GetAgent();

                return await Send(new SlimAgentContext(agent.AgentId, agent.AgencyId), messageData, notificationType, emails, templateId);
            }
            else if (apiCaller.Type == ApiCallerTypes.Admin)
                return await Send(apiCaller.Id, messageData, notificationType, emails, templateId);
            else
                return Result.Success();
        }


        public async Task<Result> Send(int adminId, JsonDocument message, NotificationTypes notificationType, string email = "", string templateId = "")
        {
            return await Send(adminId, message, notificationType, new List<string> { email }, templateId);
        }


        public async Task<Result> Send(int adminId, JsonDocument message, NotificationTypes notificationType, List<string> emails = null, string templateId = "")
        {
            return await _notificationOptionsService.GetNotificationOptions(adminId, ApiCallerTypes.Admin, null, notificationType)
                .Map(notificationOptions => BuildSettings(notificationOptions, emails, templateId))
                .Tap(sendingSettings => _internalNotificationService.AddAdminNotification(adminId, message, notificationType, sendingSettings));
        }


        public async Task<Result> Send(int adminId, DataWithCompanyInfo messageData, NotificationTypes notificationType, string email, string templateId)
        {
            return await Send(adminId, messageData, notificationType, new List<string> { email }, templateId);
        }


        public async Task<Result> Send(int adminId, DataWithCompanyInfo messageData, NotificationTypes notificationType, List<string> emails, string templateId)
        {
            return await _notificationOptionsService.GetNotificationOptions(adminId, ApiCallerTypes.Admin, null, notificationType)
                .Map(notificationOptions => BuildSettings(notificationOptions, emails, templateId))
                .Tap(sendingSettings => _internalNotificationService.AddAdminNotification(adminId, messageData, notificationType, sendingSettings));
        }


        public async Task<Result> Send(SlimAgentContext agent, JsonDocument message, NotificationTypes notificationType, string email = "", string templateId = "")
        {
            return await Send(agent, message, notificationType, new List<string> { email }, templateId);
        }


        public async Task<Result> Send(SlimAgentContext agent, JsonDocument message, NotificationTypes notificationType, List<string> emails = null, string templateId = "")
        {
            return await _notificationOptionsService.GetNotificationOptions(agent.AgentId, ApiCallerTypes.Agent, agent.AgencyId, notificationType)
                .Map(notificationOptions => BuildSettings(notificationOptions, emails, templateId))
                .Tap(sendingSettings => _internalNotificationService.AddAgentNotification(agent, message, notificationType, sendingSettings));
        }


        public async Task<Result> Send(SlimAgentContext agent, DataWithCompanyInfo messageData, NotificationTypes notificationType, string email, string templateId)
        {
            return await Send(agent, messageData, notificationType, new List<string> { email }, templateId);
        }


        public async Task<Result> Send(SlimAgentContext agent, DataWithCompanyInfo messageData, NotificationTypes notificationType, List<string> emails, string templateId)
        {
            return await _notificationOptionsService.GetNotificationOptions(agent.AgentId, ApiCallerTypes.Agent, agent.AgencyId, notificationType)
                .Map(notificationOptions => BuildSettings(notificationOptions, emails, templateId))
                .Tap(sendingSettings => _internalNotificationService.AddAgentNotification(agent, messageData, notificationType, sendingSettings));
        }


        private static Dictionary<ProtocolTypes, object> BuildSettings(SlimNotificationOptions notificationOptions, List<string> emails, string templateId)
        {
            var sendingSettings = new Dictionary<ProtocolTypes, object>();

            if ((notificationOptions.EnabledProtocols & ProtocolTypes.WebSocket) == ProtocolTypes.WebSocket)
                sendingSettings.Add(ProtocolTypes.WebSocket, new WebSocketSettings { });

            if ((notificationOptions.EnabledProtocols & ProtocolTypes.Email) == ProtocolTypes.Email)
                sendingSettings.Add(ProtocolTypes.Email, new EmailSettings { Emails = emails ?? new(0), TemplateId = templateId });

            return sendingSettings;
        }


        private readonly IInternalNotificationService _internalNotificationService;
        private readonly INotificationOptionsService _notificationOptionsService;
        private readonly IAgentContextService _agentContextService;
    }
}
