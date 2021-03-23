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


        public async Task<Result<SlimNotificationOptions>> GetNotificationOptions(NotificationTypes type, AgentContext agent)
        {
            var options = await GetOptions(type, agent.AgentId, agent.AgencyId);

            return options is null 
                ? GetDefaultOptions(type) 
                : new SlimNotificationOptions {EnabledProtocols = options.EnabledProtocols, IsMandatory = options.IsMandatory};
        }


        public Task<Result> Update(NotificationTypes type, SlimNotificationOptions options, AgentContext agentContext)
        {
            return Validate()
                .Bind(SaveOptions);


            Result<SlimNotificationOptions> Validate()
            {
                var defaultOptions = GetDefaultOptions(type);
                if (defaultOptions.IsFailure)
                    return Result.Failure<SlimNotificationOptions>(defaultOptions.Error);

                if (defaultOptions.Value.IsMandatory && options.EnabledProtocols != default)
                    return Result.Failure<SlimNotificationOptions>($"Notification type '{type}' is mandatory");

                return defaultOptions;
            }


            async Task<Result> SaveOptions(SlimNotificationOptions defaultOptions)
            {
                var entity = await GetOptions(type, agentContext.AgentId, agentContext.AgencyId);

                if (entity is null)
                {
                    _context.NotificationOptions.Add(new NotificationOptions
                    {
                        AgentId = agentContext.AgentId,
                        AgencyId = agentContext.AgencyId,
                        EnabledProtocols = options.EnabledProtocols,
                        IsMandatory = defaultOptions.IsMandatory
                    });
                }
                else
                {
                    entity.EnabledProtocols = options.EnabledProtocols;
                    _context.Update(entity);
                }

                await _context.SaveChangesAsync();
                return Result.Success();
            }
        }


        private Result<SlimNotificationOptions> GetDefaultOptions(NotificationTypes type) => 
            _defaultOptions.TryGetValue(type, out var value) 
                ? value 
                : Result.Failure<SlimNotificationOptions>($"Cannot find options for type '{type}'");


        private Task<NotificationOptions> GetOptions(NotificationTypes type, int agentId, int agencyId) 
            => _context.NotificationOptions
                .SingleOrDefaultAsync(o => o.AgencyId == agencyId && o.AgentId == agentId && o.Type == type);


        private readonly Dictionary<NotificationTypes, SlimNotificationOptions> _defaultOptions = new()
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