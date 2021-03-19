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


        public async Task<Result<SlimNotificationOption>> GetNotificationOptions(NotificationType type, AgentContext agent)
        {
            var option = await GetOption(type, agent.AgentId, agent.AgencyId);

            return option is null 
                ? GetDefaultOption(type) 
                : new SlimNotificationOption {EnabledProtocols = option.EnabledProtocols, IsMandatory = option.IsMandatory};
        }


        public Task<Result> Update(NotificationType type, SlimNotificationOption option, AgentContext agentContext)
        {
            return CheckMandatory()
                .Bind(SaveOption);


            Result<SlimNotificationOption> CheckMandatory()
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
                var entity = await GetOption(type, agentContext.AgentId, agentContext.AgencyId);

                if (entity is null)
                {
                    _context.NotificationOptions.Add(new NotificationOption
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


        private Result<SlimNotificationOption> GetDefaultOption(NotificationType type) => 
            _defaultOptions.TryGetValue(type, out var value) 
                ? value 
                : Result.Failure<SlimNotificationOption>($"Cannot find options for type '{type}'");


        private Task<NotificationOption> GetOption(NotificationType type, int agentId, int agencyId) 
            => _context.NotificationOptions
                .SingleOrDefaultAsync(o => o.AgencyId == agencyId && o.AgentId == agentId && o.Type == type);


        private readonly Dictionary<NotificationType, SlimNotificationOption> _defaultOptions = new()
        {
            {NotificationType.BookingVoucher, new() {EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = true}},
            {NotificationType.BookingInvoice, new() {EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = true}},
            {NotificationType.DeadlineApproaching, new() {EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false}},
            {NotificationType.SuccessfulPaymentReceipt, new() {EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = true}},
            {NotificationType.BookingDuePayment, new() {EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false}},
            {NotificationType.BookingCancelled, new() {EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false}},
            {NotificationType.BookingFinalized, new() {EnabledProtocols = ProtocolTypes.Email | ProtocolTypes.WebSocket, IsMandatory = false}},
            {NotificationType.BookingStatusChanged, new() {EnabledProtocols = ProtocolTypes.WebSocket, IsMandatory = false}},
        };


        private readonly EdoContext _context;
    }
}