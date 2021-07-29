using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.NotificationCenter.Models;
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


        public async Task<Result> Send(ApiCaller apiCaller, JsonDocument message, NotificationTypes notificationType)
        {
            if (apiCaller.Type == ApiCallerTypes.Agent)
            {
                var agent = await _agentContextService.GetAgent();

                return await Send(new SlimAgentContext(agent.AgentId, agent.AgencyId), message, notificationType);
            }
            else if (apiCaller.Type == ApiCallerTypes.Admin)
                return await Send(new SlimAdminContext(apiCaller.Id), message, notificationType);
            else
                return Result.Success();
        }


        public async Task<Result> Send(ApiCaller apiCaller, DataWithCompanyInfo messageData, NotificationTypes notificationType, string email, string templateId)
            => await Send(apiCaller, messageData, notificationType, new List<string> { email }, templateId);


        public async Task<Result> Send(ApiCaller apiCaller, DataWithCompanyInfo messageData, NotificationTypes notificationType, List<string> emails, string templateId)
        {
            if (apiCaller.Type == ApiCallerTypes.Agent)
            {
                var agent = await _agentContextService.GetAgent();

                return await Send(new SlimAgentContext(agent.AgentId, agent.AgencyId), messageData, notificationType, emails, templateId);
            }
            else if (apiCaller.Type == ApiCallerTypes.Admin)
                return await Send(new SlimAdminContext(apiCaller.Id), messageData, notificationType, emails, templateId);
            else if (apiCaller.Type == ApiCallerTypes.PropertyOwner)
            {
                return await _notificationOptionsService.GetNotificationOptions(NoUserId, apiCaller.Type, NoAgencyId, notificationType)
                    .Map(notificationOptions => BuildSettings(notificationOptions, emails, templateId))
                    .Tap(sendingSettings => _internalNotificationService.AddPropertyOwnerNotification(messageData, notificationType, sendingSettings));
            }
            else
                return Result.Success();
        }


        public async Task<Result> Send(SlimAdminContext admin, JsonDocument message, NotificationTypes notificationType)
            => await _notificationOptionsService.GetNotificationOptions(admin.AdminId, ApiCallerTypes.Admin, NoAgencyId, notificationType)
                .Map(notificationOptions => BuildSettings(notificationOptions, null, string.Empty))
                .Tap(sendingSettings => _internalNotificationService.AddAdminNotification(admin, message, notificationType, sendingSettings));


        public async Task<Result> Send(SlimAdminContext admin, DataWithCompanyInfo messageData, NotificationTypes notificationType, string email, string templateId)
            => await Send(admin, messageData, notificationType, new List<string> { email }, templateId);


        public async Task<Result> Send(SlimAdminContext admin, DataWithCompanyInfo messageData, NotificationTypes notificationType, List<string> emails, string templateId)
        {
            return await _notificationOptionsService.GetNotificationOptions(admin.AdminId, ApiCallerTypes.Admin, NoAgencyId, notificationType)
                .Map(notificationOptions => BuildSettings(notificationOptions, emails, templateId))
                .Tap(sendingSettings => _internalNotificationService.AddAdminNotification(admin, messageData, notificationType, sendingSettings));
        }


        public async Task<Result> Send(SlimAgentContext agent, JsonDocument message, NotificationTypes notificationType)
            => await _notificationOptionsService.GetNotificationOptions(agent.AgentId, ApiCallerTypes.Agent, agent.AgencyId, notificationType)
                .Map(notificationOptions => BuildSettings(notificationOptions, null, string.Empty))
                .Tap(sendingSettings => _internalNotificationService.AddAgentNotification(agent, message, notificationType, sendingSettings));


        public async Task<Result> Send(SlimAgentContext agent, DataWithCompanyInfo messageData, NotificationTypes notificationType, string email, string templateId)
            => await Send(agent, messageData, notificationType, new List<string> { email }, templateId);


        public async Task<Result> Send(SlimAgentContext agent, DataWithCompanyInfo messageData, NotificationTypes notificationType, List<string> emails, string templateId)
            => await _notificationOptionsService.GetNotificationOptions(agent.AgentId, ApiCallerTypes.Agent, agent.AgencyId, notificationType)
                .Map(notificationOptions => BuildSettings(notificationOptions, emails, templateId))
                .Tap(sendingSettings => _internalNotificationService.AddAgentNotification(agent, messageData, notificationType, sendingSettings));


        public async Task<Result> Send(DataWithCompanyInfo messageData, NotificationTypes notificationType, string email, string templateId)
            => await Send(new SlimAgentContext(agentId: 0, agencyId: 0), messageData, notificationType, new List<string> { email }, templateId);


        public async Task<Result> Send(DataWithCompanyInfo messageData, NotificationTypes notificationType, List<string> emails, string templateId)
            => await Send(new SlimAdminContext(adminId: 0), messageData, notificationType, emails, templateId);


        public async Task<List<SlimNotification>> Get(SlimAgentContext agent, int skip, int top)
            => await _internalNotificationService.Get(ReceiverTypes.AgentApp, agent.AgentId, agent.AgencyId, skip, top);
        

        public async Task<List<SlimNotification>> Get(SlimAdminContext admin, int skip, int top)
            => await _internalNotificationService.Get(ReceiverTypes.AdminPanel, admin.AdminId, null, skip, top);


        private static Dictionary<ProtocolTypes, object> BuildSettings(SlimNotificationOptions notificationOptions, List<string> emails, string templateId)
        {
            var sendingSettings = new Dictionary<ProtocolTypes, object>();

            if ((notificationOptions.EnabledProtocols & ProtocolTypes.WebSocket) == ProtocolTypes.WebSocket)
                sendingSettings.Add(ProtocolTypes.WebSocket, new WebSocketSettings { });

            if ((notificationOptions.EnabledProtocols & ProtocolTypes.Email) == ProtocolTypes.Email)
                sendingSettings.Add(ProtocolTypes.Email, new EmailSettings { Emails = emails ?? new(0), TemplateId = templateId });

            return sendingSettings;
        }


        private const int NoUserId = 0;
        private readonly int? NoAgencyId = null;

        private readonly IInternalNotificationService _internalNotificationService;
        private readonly INotificationOptionsService _notificationOptionsService;
        private readonly IAgentContextService _agentContextService;
    }
}
