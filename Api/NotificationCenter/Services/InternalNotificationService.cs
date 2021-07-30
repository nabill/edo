using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.Notifications.Models;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;
using HappyTravel.Edo.Api.NotificationCenter.Hubs;
using HappyTravel.Edo.Api.NotificationCenter.Models;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Notifications.Infrastructure;

namespace HappyTravel.Edo.Api.NotificationCenter.Services
{
    public class InternalNotificationService : IInternalNotificationService
    {
        public InternalNotificationService(EdoContext context,
            IHubContext<AgentNotificationHub, INotificationClient> agentNotificationHub,
            IHubContext<AdminNotificationHub, INotificationClient> adminNotificationHub,
            MailSenderWithCompanyInfo mailSender,
            IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _agentNotificationHub = agentNotificationHub;
            _adminNotificationHub = adminNotificationHub;
            _mailSender = mailSender;
            _dateTimeProvider = dateTimeProvider;
        }


        public async Task AddAdminNotification(SlimAdminContext admin, JsonDocument message, NotificationTypes notificationType, Dictionary<ProtocolTypes, object> sendingSettings)
        {
            var notification = new Notification
            {
                Receiver = ReceiverTypes.AdminPanel,
                UserId = admin.AdminId,
                AgencyId = null,
                Message = message,
                Type = notificationType,
                SendingSettings = sendingSettings
            };

            await SaveAndSend(notification, null);
        }


        public async Task AddAdminNotification(SlimAdminContext admin, DataWithCompanyInfo messageData, NotificationTypes notificationType, Dictionary<ProtocolTypes, object> sendingSettings)
        {
            var notification = new Notification
            {
                Receiver = ReceiverTypes.AdminPanel,
                UserId = admin.AdminId,
                AgencyId = null,
                Message = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes((object)messageData, new(JsonSerializerDefaults.Web))),
                Type = notificationType,
                SendingSettings = sendingSettings
            };

            await SaveAndSend(notification, messageData);
        }


        public async Task AddAgentNotification(SlimAgentContext agent, JsonDocument message, NotificationTypes notificationType, Dictionary<ProtocolTypes, object> sendingSettings)
        {
            var notification = new Notification
            {
                Receiver = ReceiverTypes.AgentApp,
                UserId = agent.AgentId,
                AgencyId = agent.AgencyId,
                Message = message,
                Type = notificationType,
                SendingSettings = sendingSettings
            };

            await SaveAndSend(notification, null);
        }


        public async Task AddAgentNotification(SlimAgentContext agent, DataWithCompanyInfo messageData, NotificationTypes notificationType, Dictionary<ProtocolTypes, object> sendingSettings)
        {
            var notification = new Notification
            {
                Receiver = ReceiverTypes.AgentApp,
                UserId = agent.AgentId,
                AgencyId = agent.AgencyId,
                Message = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes((object)messageData, new(JsonSerializerDefaults.Web))),
                Type = notificationType,
                SendingSettings = sendingSettings
            };

            await SaveAndSend(notification, messageData);
        }


        public async Task AddPropertyOwnerNotification(DataWithCompanyInfo messageData, NotificationTypes notificationType, Dictionary<ProtocolTypes, object> sendingSettings)
        {
            var notification = new Notification
            {
                Receiver = ReceiverTypes.PropertyOwner,
                UserId = 0,
                AgencyId = null,
                Message = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes((object)messageData, new(JsonSerializerDefaults.Web))),
                Type = notificationType,
                SendingSettings = sendingSettings
            };

            await SaveAndSend(notification, messageData);
        }


        public async Task<List<SlimNotification>> Get(ReceiverTypes receiver, int userId, int? agencyId, int skip, int top)
            => await _context.Notifications
                .Where(n => n.Receiver == receiver && n.UserId == userId && n.AgencyId == agencyId)
                .Skip(skip)
                .Take(top)
                .Select(n => new SlimNotification
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    AgencyId = n.AgencyId,
                    Message = n.Message.ToJsonString(),
                    Type = n.Type,
                    SendingStatus = n.SendingStatus,
                    Created = n.Created,
                    Received = n.Received,
                    Read = n.Read
                })
                .ToListAsync();


        private async Task SaveAndSend(Notifications.Models.Notification notification, DataWithCompanyInfo messageData)
        {
            var notificationId = await Save(notification);
            await Send(notification, notificationId, messageData);
        }


        private async Task<int> Save(Notifications.Models.Notification notification)
        {
            var entry = _context.Notifications.Add(new Data.Notifications.Notification
            {
                Receiver = notification.Receiver,
                UserId = notification.UserId,
                AgencyId = notification.AgencyId,
                Message = notification.Message,
                Type = notification.Type,
                SendingSettings = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(notification.SendingSettings, new(JsonSerializerDefaults.Web))),
                SendingStatus = SendingStatuses.Sent,
                Created = _dateTimeProvider.UtcNow()
            });
            await _context.SaveChangesAsync();

            return entry.Entity.Id;
        }


        private async Task Send(Notifications.Models.Notification notification, int notificationId, DataWithCompanyInfo messageData)
        {
            var tasks = new List<Task>();

            foreach (var (protocol, settings) in notification.SendingSettings)
            {
                var task = protocol switch
                {
                    ProtocolTypes.Email when settings is EmailSettings emailSettings && messageData is not null
                        => SendEmail(emailSettings, messageData),

                    ProtocolTypes.WebSocket when settings is WebSocketSettings webSocketSettings
                        => notification.Receiver switch
                        {
                            ReceiverTypes.AgentApp
                                => SendMessageToAgent(notification.UserId, notification.AgencyId, notificationId, notification.Type, notification.Message),

                            ReceiverTypes.AdminPanel
                                => SendMessageToAdmin(notification.UserId, notificationId, notification.Type, notification.Message),

                            _ => throw new ArgumentException($"Unsupported receiver '{notification.Receiver}' for notification")
                        },

                    _ => throw new ArgumentException($"Unsupported protocol '{protocol}' or incorrect settings type")
                };

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }


        private async Task<Result> SendEmail(EmailSettings settings, DataWithCompanyInfo messageData)
            => await _mailSender.Send(settings.TemplateId, settings.Emails, messageData);


        private async Task SendMessageToAgent(int userId, int? agencyId, int messageId, NotificationTypes notificationType, JsonDocument message)
            => await _agentNotificationHub.Clients
                .Group($"{agencyId}-{userId}")
                .ReceiveMessage(messageId, notificationType.ToString(), message);


        private async Task SendMessageToAdmin(int userId, int messageId, NotificationTypes notificationType, JsonDocument message)
            => await _adminNotificationHub.Clients
                .Group($"admin-{userId}")
                .ReceiveMessage(messageId, notificationType.ToString(), message);


        private readonly EdoContext _context;
        private readonly IHubContext<AgentNotificationHub, INotificationClient> _agentNotificationHub;
        private readonly IHubContext<AdminNotificationHub, INotificationClient> _adminNotificationHub;
        private readonly MailSenderWithCompanyInfo _mailSender;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}