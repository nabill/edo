using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Notifications;
using HappyTravel.Edo.Common.Enums.Notifications;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Notifications;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Notifications
{
    public class NotificationOptionsService : INotificationOptionsService
    {
        public NotificationOptionsService(EdoContext context)
        {
            _context = context;
        }


        public async Task<Result<SlimNotificationOption>> GetNotificationOptions(NotificationTypes type, AgentContext agent)
        {
            var options = await GetOptions(type, agent.AgentId, agent.AgencyId);

            return options is null 
                ? GetDefaultOption(type) 
                : new SlimNotificationOption {EnabledProtocols = options.EnabledProtocols, IsMandatory = options.IsMandatory};
        }


        public Task<Result> Update(NotificationTypes type, SlimNotificationOption option, AgentContext agentContext)
        {
            return Validate()
                .Bind(SaveOption);


            Result<SlimNotificationOption> Validate()
            {
                var defaultOption = GetDefaultOption(type);
                if (defaultOption.IsFailure)
                    return Result.Failure<SlimNotificationOption>(defaultOption.Error);

                if (defaultOption.Value.IsMandatory && option.EnabledProtocols != default)
                    return Result.Failure<SlimNotificationOption>($"Notification type '{type}' is mandatory");

                return defaultOption;
            }


            async Task<Result> SaveOption(SlimNotificationOption defaultOption)
            {
                var entity = await GetOptions(type, agentContext.AgentId, agentContext.AgencyId);

                if (entity is null)
                {
                    _context.NotificationOptions.Add(new NotificationOptions
                    {
                        AgentId = agentContext.AgentId,
                        AgencyId = agentContext.AgencyId,
                        EnabledProtocols = option.EnabledProtocols,
                        IsMandatory = defaultOption.IsMandatory
                    });
                }
                else
                {
                    entity.EnabledProtocols = option.EnabledProtocols;
                    _context.Update(entity);
                }

                await _context.SaveChangesAsync();
                return Result.Success();
            }
        }


        private Result<SlimNotificationOption> GetDefaultOption(NotificationTypes type) => 
            _defaultOptions.TryGetValue(type, out var value) 
                ? value 
                : Result.Failure<SlimNotificationOption>($"Cannot find options for type '{type}'");


        private Task<NotificationOptions> GetOptions(NotificationTypes type, int agentId, int agencyId) 
            => _context.NotificationOptions
                .SingleOrDefaultAsync(o => o.AgencyId == agencyId && o.AgentId == agentId && o.Type == type);


        private readonly Dictionary<NotificationTypes, SlimNotificationOption> _defaultOptions = new()
        {
            {NotificationTypes.BookingVoucher, new() {EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = true}},
            {NotificationTypes.BookingInvoice, new() {EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = true}},
            {NotificationTypes.DeadlineApproaching, new() {EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false}},
            {NotificationTypes.SuccessfulPaymentReceipt, new() {EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = true}},
            {NotificationTypes.BookingDuePaymentDate, new() {EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false}},
            {NotificationTypes.BookingCancelled, new() {EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false}},
            {NotificationTypes.BookingFinalized, new() {EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false}},
            {NotificationTypes.BookingStatusChanged, new() {EnabledProtocols = ProtocolTypes.WebSocket, IsMandatory = false}},
        };


        private readonly EdoContext _context;
    }
}