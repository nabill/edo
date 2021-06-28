using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Notifications;
using Microsoft.EntityFrameworkCore;
using HappyTravel.Edo.Notifications.Models;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Api.Models.Agents;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.NotificationCenter.Models;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;

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

            return options is null 
                ? await TryGetDefaultOptions(notificationType, userType) 
                : new SlimNotificationOptions {EnabledProtocols = options.EnabledProtocols, IsMandatory = options.IsMandatory};
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
            var receiver = GetReceiver(userType);
            var options = await _context.DefaultNotificationOptions.SingleOrDefaultAsync(o => o.Type == type);

            if (options is null)
                return Result.Failure<SlimNotificationOptions>($"Cannot find notification options for the type '{type}'");

            return options.EnabledReceivers.HasFlag(receiver)
                ? new SlimNotificationOptions(options.EnabledProtocols, options.IsMandatory, options.EnabledReceivers)
                : Result.Failure<SlimNotificationOptions>($"Cannot find notification options for the type '{type}' and the receiver '{receiver}'");


            static ReceiverTypes GetReceiver(ApiCallerTypes userType)
                => userType switch
                {
                    ApiCallerTypes.Admin => ReceiverTypes.AdminPanel,
                    ApiCallerTypes.Agent => ReceiverTypes.AgentApp,
                    _ => throw new System.NotImplementedException()
                };
        }


        private async Task<Dictionary<NotificationTypes, SlimNotificationOptions>> GetDefaultOptions(ReceiverTypes receiver)
            => await _context.DefaultNotificationOptions
                .Where(o => o.EnabledReceivers.HasFlag(receiver))
                .ToDictionaryAsync(o => o.Type, o => new SlimNotificationOptions(o.EnabledProtocols, o.IsMandatory, o.EnabledReceivers));


        private readonly EdoContext _context;
    }
}