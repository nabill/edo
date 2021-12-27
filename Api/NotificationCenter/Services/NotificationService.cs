using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.NotificationCenter.Models;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.Notifications.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.NotificationCenter.Services
{
    public class NotificationService : INotificationService
    {
        public NotificationService(IInternalNotificationService internalNotificationService,
            INotificationOptionsService notificationOptionsService, IAgentContextService agentContextService, EdoContext context)
        {
            _internalNotificationService = internalNotificationService;
            _notificationOptionsService = notificationOptionsService;
            _agentContextService = agentContextService;
            _context = context;
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


        public async Task<Result> Send(ApiCaller apiCaller, DataWithCompanyInfo messageData, NotificationTypes notificationType, string email)
            => await Send(apiCaller, messageData, notificationType, new List<string> { email });


        public async Task<Result> Send(ApiCaller apiCaller, DataWithCompanyInfo messageData, NotificationTypes notificationType, List<string> emails)
        {
            if (apiCaller.Type == ApiCallerTypes.Agent)
            {
                var agent = await _agentContextService.GetAgent();

                return await Send(new SlimAgentContext(agent.AgentId, agent.AgencyId), messageData, notificationType, emails);
            }
            else if (apiCaller.Type == ApiCallerTypes.Admin)
                return await Send(new SlimAdminContext(apiCaller.Id), messageData, notificationType, emails);
            else if (apiCaller.Type == ApiCallerTypes.PropertyOwner)
            {
                return await _notificationOptionsService.GetNotificationOptions(NoUserId, apiCaller.Type, NoAgencyId, notificationType)
                    .Map(notificationOptions => BuildSettings(notificationOptions, emails))
                    .Tap(sendingSettings => _internalNotificationService.AddPropertyOwnerNotification(messageData, notificationType, sendingSettings));
            }
            else
                return Result.Success();
        }


        public async Task<Result> Send(SlimAdminContext admin, JsonDocument message, NotificationTypes notificationType)
            => await _notificationOptionsService.GetNotificationOptions(admin.AdminId, ApiCallerTypes.Admin, NoAgencyId, notificationType)
                .Map(notificationOptions => BuildSettings(notificationOptions, null))
                .Tap(sendingSettings => _internalNotificationService.AddAdminNotification(admin, message, notificationType, sendingSettings));


        public async Task<Result> Send(SlimAdminContext admin, DataWithCompanyInfo messageData, NotificationTypes notificationType, string email)
            => await Send(admin, messageData, notificationType, new List<string> { email });


        public async Task<Result> Send(SlimAdminContext admin, DataWithCompanyInfo messageData, NotificationTypes notificationType, List<string> emails)
        {
            return await _notificationOptionsService.GetNotificationOptions(admin.AdminId, ApiCallerTypes.Admin, NoAgencyId, notificationType)
                .Map(notificationOptions => BuildSettings(notificationOptions, emails))
                .Tap(sendingSettings => _internalNotificationService.AddAdminNotification(admin, messageData, notificationType, sendingSettings));
        }


        public async Task<Result> Send(SlimAgentContext agent, JsonDocument message, NotificationTypes notificationType)
            => await _notificationOptionsService.GetNotificationOptions(agent.AgentId, ApiCallerTypes.Agent, agent.AgencyId, notificationType)
                .Map(notificationOptions => BuildSettings(notificationOptions, null))
                .Tap(sendingSettings => _internalNotificationService.AddAgentNotification(agent, message, notificationType, sendingSettings));


        public async Task<Result> Send(SlimAgentContext agent, DataWithCompanyInfo messageData, NotificationTypes notificationType, string email)
            => await Send(agent, messageData, notificationType, new List<string> { email });


        public async Task<Result> Send(SlimAgentContext agent, DataWithCompanyInfo messageData, NotificationTypes notificationType, List<string> emails)
            => await _notificationOptionsService.GetNotificationOptions(agent.AgentId, ApiCallerTypes.Agent, agent.AgencyId, notificationType)
                .Map(notificationOptions => BuildSettings(notificationOptions, emails))
                .Tap(sendingSettings => _internalNotificationService.AddAgentNotification(agent, messageData, notificationType, sendingSettings));


        public async Task<Result> Send(DataWithCompanyInfo messageData, NotificationTypes notificationType, string email)
            => await Send(new SlimAgentContext(agentId: 0, agencyId: 0), messageData, notificationType, new List<string> { email });


        public async Task<Result> Send(DataWithCompanyInfo messageData, NotificationTypes notificationType, List<string> emails)
            => await Send(new SlimAdminContext(adminId: 0), messageData, notificationType, emails);


        public async Task<Result> Send(DataWithCompanyInfo messageData, NotificationTypes notificationType)
        {
            return await GetRecipients(notificationType)
                .Bind(GetNotificationOptions)
                .Bind(BuildSettings)
                .Tap(AddNotifications);


            async Task<Result<Dictionary<int, string>>> GetRecipients(NotificationTypes notificationType)
            {
                var roleIds = await _context.AdministratorRoles.Where(r => r.NotificationTypes.Contains(notificationType))
                    .Select(r => r.Id)
                    .ToListAsync();
                var recipients = new Dictionary<int, string>();

                foreach (var roleId in roleIds)
                {
                    recipients = (Dictionary<int, string>)recipients.Union(await _context.Administrators.Where(a => a.AdministratorRoleIds.Contains(roleId))
                        .ToDictionaryAsync(a => a.Id, a => a.Email));
                }

                return recipients;
            }


            async Task<Result<List<RecipientWithNotificationOptions>>> GetNotificationOptions(Dictionary<int, string> recipients)
                => await _notificationOptionsService.GetNotificationOptions(recipients, notificationType);


            static Result<List<RecipientWithSendingSettings>> BuildSettings(List<RecipientWithNotificationOptions> recipientWithNotificationOptions)
            {
                var recipientsWithSendingSettings = new List<RecipientWithSendingSettings>(recipientWithNotificationOptions.Count);

                foreach (var recipient in recipientWithNotificationOptions)
                {
                    var sendingSettings = new Dictionary<ProtocolTypes, object>();

                    if ((recipient.NotificationOptions?.EnabledProtocols & ProtocolTypes.WebSocket) == ProtocolTypes.WebSocket)
                        sendingSettings.Add(ProtocolTypes.WebSocket, new WebSocketSettings { });

                    if ((recipient.NotificationOptions?.EnabledProtocols & ProtocolTypes.Email) == ProtocolTypes.Email)
                        sendingSettings.Add(ProtocolTypes.Email, new EmailSettings 
                            { 
                                Emails = { recipient.Email }, 
                                TemplateId = recipient.NotificationOptions?.EmailTemplateId 
                            });

                    recipientsWithSendingSettings.Add(new RecipientWithSendingSettings 
                    { 
                        RecipientId = recipient.RecipientId,
                        SendingSettings = sendingSettings
                    });
                }

                return recipientsWithSendingSettings;
            }


            async Task AddNotifications(List<RecipientWithSendingSettings> recipientsWithSendingSettings)
            {
                await _internalNotificationService.AddAdminNotifications(messageData, notificationType, recipientsWithSendingSettings);
            }
        }


        public async Task<List<SlimNotification>> Get(SlimAgentContext agent, int skip, int top)
            => await _internalNotificationService.Get(ReceiverTypes.AgentApp, agent.AgentId, agent.AgencyId, skip, top);
        

        public async Task<List<SlimNotification>> Get(SlimAdminContext admin, int skip, int top)
            => await _internalNotificationService.Get(ReceiverTypes.AdminPanel, admin.AdminId, null, skip, top);


        private static Dictionary<ProtocolTypes, object> BuildSettings(SlimNotificationOptions notificationOptions, List<string> emails)
        {
            var sendingSettings = new Dictionary<ProtocolTypes, object>();

            if ((notificationOptions.EnabledProtocols & ProtocolTypes.WebSocket) == ProtocolTypes.WebSocket)
                sendingSettings.Add(ProtocolTypes.WebSocket, new WebSocketSettings { });

            if ((notificationOptions.EnabledProtocols & ProtocolTypes.Email) == ProtocolTypes.Email)
                sendingSettings.Add(ProtocolTypes.Email, new EmailSettings { Emails = emails ?? new(0), TemplateId = notificationOptions.EmailTemplateId });

            return sendingSettings;
        }


        private const int NoUserId = 0;
        private readonly int? NoAgencyId = null;

        private readonly IInternalNotificationService _internalNotificationService;
        private readonly INotificationOptionsService _notificationOptionsService;
        private readonly IAgentContextService _agentContextService;
        private readonly EdoContext _context;
    }
}
