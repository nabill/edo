using System.Collections.Generic;
using System.Linq;
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


        public async Task<Result<NotificationOptionSlim>> GetNotificationOptions(NotificationType type, AgentContext agent)
        {
            var option = await GetOption(type, agent.AgentId, agent.AgencyId);

            return option is null 
                ? GetDefaultOption(type) 
                : new NotificationOptionSlim {EnabledProtocols = option.EnabledProtocols, IsMandatory = option.IsMandatory};
        }


        public Task<Result> Update(NotificationType type, Models.Notifications.NotificationOptionSlim option, AgentContext agentContext)
        {
            return CheckMandatory()
                .Bind(SaveOption);


            Result<NotificationOptionSlim> CheckMandatory()
            {
                var defaultOption = GetDefaultOption(type);
                if (defaultOption.IsFailure)
                    return Result.Failure<NotificationOptionSlim>(defaultOption.Error);

                if (defaultOption.Value.IsMandatory && option.EnabledProtocols != default)
                    return Result.Failure<NotificationOptionSlim>($"Notification type '{type}' is mandatory");

                return defaultOption;
            }


            async Task<Result> SaveOption(NotificationOptionSlim defaultOption)
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


        private Result<NotificationOptionSlim> GetDefaultOption(NotificationType type) => 
            _defaultOptions.TryGetValue(type, out var value) 
                ? value 
                : Result.Failure<NotificationOptionSlim>($"Cannot find options for type '{type}'");


        private Task<NotificationOption> GetOption(NotificationType type, int agentId, int agencyId) 
            => _context.NotificationOptions
                .SingleOrDefaultAsync(o => o.AgencyId == agencyId && o.AgentId == agentId && o.Type == type);


        private readonly Dictionary<NotificationType, Models.Notifications.NotificationOptionSlim> _defaultOptions = new()
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