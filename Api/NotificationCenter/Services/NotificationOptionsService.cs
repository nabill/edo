using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.NotificationCenter.Infrastructure;
using HappyTravel.Edo.Api.NotificationCenter.Models;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Notifications;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.Notifications.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.NotificationCenter.Services
{
    public class NotificationOptionsService : INotificationOptionsService
    {
        public NotificationOptionsService(EdoContext context)
        {
            _context = context;
        }


        public async Task<Result<SlimNotificationOptions>> GetNotificationOptions(int userId, ApiCallerTypes userType, int? agencyId, NotificationTypes notificationType)
        {
            var options = await GetOptions(userId, userType, agencyId, notificationType);

            var (_, isFailure, defaultOptions, error) = await TryGetDefaultOptions(notificationType, userType);
            if (isFailure)
                return Result.Failure<SlimNotificationOptions>(error);

            return options is null 
                ? defaultOptions 
                : new SlimNotificationOptions {EnabledProtocols = options.EnabledProtocols, IsMandatory = defaultOptions.IsMandatory, 
                    EmailTemplateId = defaultOptions.EmailTemplateId};
        }


        public async Task<Result<List<RecipientWithNotificationOptions>>> GetNotificationOptions(Dictionary<int, string> recipients, NotificationTypes notificationType)
        {
            var recipientsWithNotificationOptions = await GetOptions(recipients, notificationType);

            var (_, isFailure, defaultOptions, error) = await TryGetDefaultOptions(notificationType, ApiCallerTypes.Admin);
            if (isFailure)
                return Result.Failure<List<RecipientWithNotificationOptions>>(error);

            foreach (var recipient in recipientsWithNotificationOptions)
            {
                recipient.NotificationOptions = (recipient.NotificationOptions is null)
                    ? defaultOptions
                    : new SlimNotificationOptions(enabledProtocols: recipient.NotificationOptions?.EnabledProtocols ?? ProtocolTypes.None,
                        isMandatory: defaultOptions.IsMandatory,
                        enabledReceivers: recipient.NotificationOptions?.EnabledReceivers ?? ReceiverTypes.None,
                        emailTemplateId: defaultOptions.EmailTemplateId);
            }

            return recipientsWithNotificationOptions;
        }


        public async Task<Dictionary<NotificationTypes, NotificationSettings>> Get(SlimAgentContext agent)
        {
            var agentOptions = await _context.NotificationOptions
                .Where(o => o.AgencyId == agent.AgencyId && o.UserId == agent.AgentId && o.UserType == ApiCallerTypes.Agent)
                .ToListAsync();

            return await GetMaterializedOptions(agentOptions, ReceiverTypes.AgentApp);
        }


        public async Task<Dictionary<NotificationTypes, NotificationSettings>> Get(SlimAdminContext admin)
        {
            var adminOptions = await _context.NotificationOptions
                .Where(o => o.AgencyId == null && o.UserId == admin.AdminId && o.UserType == ApiCallerTypes.Admin)
                .ToListAsync();

            return await GetMaterializedOptions (adminOptions, ReceiverTypes.AdminPanel);
        }


        public async Task<Result> Update(SlimAgentContext agent, Dictionary<NotificationTypes, NotificationSettings> notificationOptions)
            => await Update(agent.AgentId, ApiCallerTypes.Agent, agent.AgencyId, notificationOptions);


        public async Task<Result> Update(SlimAdminContext admin, Dictionary<NotificationTypes, NotificationSettings> notificationOptions)
            => await Update(admin.AdminId, ApiCallerTypes.Admin, null, notificationOptions);


        private async Task<Result> Update(int userId, ApiCallerTypes userType, int? agencyId, Dictionary<NotificationTypes, NotificationSettings> notificationOptions)
        {
            return await Result.Success()
                .BindWithTransaction(_context, () => UpdateAll()
                    .Tap(() => SaveAll()));


            async Task<Result> UpdateAll()
            {
                foreach (var option in notificationOptions)
                {
                    var result = await Validate(option)
                        .Bind(defaultOptions => Update(option, defaultOptions));

                    if (result.IsFailure)
                        return Result.Failure(result.Error);
                }

                return Result.Success();


                async Task<Result<SlimNotificationOptions>> Validate(KeyValuePair<NotificationTypes, NotificationSettings> option)
                {
                    var defaultOptions = await TryGetDefaultOptions(option.Key, userType);
                    if (defaultOptions.IsFailure)
                        return Result.Failure<SlimNotificationOptions>(defaultOptions.Error);

                    if (defaultOptions.Value.IsMandatory && defaultOptions.Value.EnabledProtocols != GetEnabledProtocols(option.Value))
                        return Result.Failure<SlimNotificationOptions>($"Notification type '{option.Key}' is mandatory");

                    return defaultOptions;
                }


                async Task<Result> Update(KeyValuePair<NotificationTypes, NotificationSettings> option, SlimNotificationOptions defaultOptions)
                {
                    var entity = await GetOptions(userId, userType, agencyId, option.Key);

                    if (entity is null)
                    {
                        _context.NotificationOptions.Add(new NotificationOptions
                        {
                            UserId = userId,
                            UserType = userType,
                            AgencyId = agencyId,
                            Type = option.Key,
                            EnabledProtocols = GetEnabledProtocols(option.Value),
                            IsMandatory = defaultOptions.IsMandatory
                        });
                    }
                    else
                    {
                        entity.EnabledProtocols = GetEnabledProtocols(option.Value);
                        entity.IsMandatory = defaultOptions.IsMandatory;
                        _context.Update(entity);
                    }

                    return Result.Success();
                }


                ProtocolTypes GetEnabledProtocols(NotificationSettings options)
                {
                    ProtocolTypes protocols = 0;

                    foreach (var (protocol, isEnabled) in options.EnabledProtocols)
                    {
                        if (isEnabled)
                            protocols |= protocol;
                    }

                    return protocols;
                }
            }


            async Task SaveAll()
            {
                await _context.SaveChangesAsync();
            }
        }


        private Task<NotificationOptions> GetOptions(int userId, ApiCallerTypes userType, int? agencyId, NotificationTypes notificationType) 
            => _context.NotificationOptions
                .SingleOrDefaultAsync(o => o.UserId == userId && o.UserType == userType && o.AgencyId == agencyId && o.Type == notificationType);


        private async Task<List<RecipientWithNotificationOptions>> GetOptions(Dictionary<int, string> recipients, NotificationTypes notificationType)
        {
            var admins = await _context.NotificationOptions.Where(o => o.Type == notificationType && o.UserType == ApiCallerTypes.Admin
                && recipients.Keys.Contains(o.UserId))
                .ToListAsync();

            return recipients.Select(r => new RecipientWithNotificationOptions 
                { 
                    RecipientId = r.Key, 
                    Email = r.Value, 
                    NotificationOptions = admins.SingleOrDefault(a => a.UserId == r.Key)?.ToSlimNotificationOptions()
                })
                .ToList();
        }


        private async Task<Dictionary<NotificationTypes, NotificationSettings>> GetMaterializedOptions(List<NotificationOptions> userOptions, ReceiverTypes receiver)
        {
            var defaultOptions = await GetDefaultOptions(receiver);
            var materializedSettings = new Dictionary<NotificationTypes, NotificationSettings>();

            foreach (var option in defaultOptions)
            {
                var userOption = userOptions.SingleOrDefault(no => no.Type == option.Key);

                var enabledProtocols = new Dictionary<ProtocolTypes, bool>();
                if (option.Value.EnabledProtocols.HasFlag(ProtocolTypes.Email))
                {
                    enabledProtocols.Add(ProtocolTypes.Email, userOption is null || option.Value.IsMandatory || userOption.EnabledProtocols.HasFlag(ProtocolTypes.Email));
                }
                if (option.Value.EnabledProtocols.HasFlag(ProtocolTypes.WebSocket))
                {
                    enabledProtocols.Add(ProtocolTypes.WebSocket, userOption is null || option.Value.IsMandatory || userOption.EnabledProtocols.HasFlag(ProtocolTypes.WebSocket));
                }

                materializedSettings.Add(option.Key, new NotificationSettings { EnabledProtocols = enabledProtocols, IsMandatory = option.Value.IsMandatory });
            }

            return materializedSettings;
        }


        private async Task<Result<SlimNotificationOptions>> TryGetDefaultOptions(NotificationTypes type, ApiCallerTypes userType)
        {
            var receiver = userType.ToReceiverType();
            var options = await _context.DefaultNotificationOptions.SingleOrDefaultAsync(o => o.Type == type);

            if (options is null)
                return Result.Failure<SlimNotificationOptions>($"Cannot find notification options for the type '{type}'");

            var emailTemplateId = userType switch
            {
                ApiCallerTypes.Agent => options.AgentEmailTemplateId,
                ApiCallerTypes.Admin => options.AdminEmailTemplateId,
                ApiCallerTypes.PropertyOwner => options.PropertyOwnerEmailTemplateId,
                _ => throw new NotImplementedException()
            };

            return options.EnabledReceivers.HasFlag(receiver)
                ? new SlimNotificationOptions(enabledProtocols: options.EnabledProtocols, 
                    isMandatory: options.IsMandatory, 
                    enabledReceivers: options.EnabledReceivers, 
                    emailTemplateId: emailTemplateId)
                : Result.Failure<SlimNotificationOptions>($"Cannot find notification options for the type '{type}' and the receiver '{receiver}'");
        }


        private async Task<Dictionary<NotificationTypes, SlimNotificationOptions>> GetDefaultOptions(ReceiverTypes receiver)
            => await _context.DefaultNotificationOptions
                .Where(o => o.EnabledReceivers.HasFlag(receiver))
                .ToDictionaryAsync(o => o.Type, o => new SlimNotificationOptions(o.EnabledProtocols, o.IsMandatory, o.EnabledReceivers, GetTemplateId(o, receiver)));


        private static string GetTemplateId(DefaultNotificationOptions options, ReceiverTypes receiver)
            => receiver switch
            {
                ReceiverTypes.AgentApp => options.AgentEmailTemplateId,
                ReceiverTypes.AdminPanel => options.AdminEmailTemplateId,
                ReceiverTypes.PropertyOwner => options.PropertyOwnerEmailTemplateId,
                _ => throw new System.NotImplementedException()
            };


        private readonly EdoContext _context;
    }
}