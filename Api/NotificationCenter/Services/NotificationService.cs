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

namespace HappyTravel.Edo.Api.NotificationCenter.Services
{
    public class NotificationService : INotificationService
    {
        public NotificationService(EdoContext context, IHubContext<NotificationHub, INotificationClient> notificationHub)
        {
            _context = context;
            _notificationHub = notificationHub;
        }
        

        public async Task Add(Notifications.Models.Notification notification)
        {
            var entry = _context.Notifications.Add(new Data.Notifications.Notification
            {
                Receiver = notification.Receiver,
                UserId = notification.UserId,
                Message = notification.Message.ToString(),
                Type = notification.Type,
                SendingSettings = notification.SendingSettings,
                Created = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var tasks = new List<Task>();

            foreach (var (protocol, settings) in notification.SendingSettings)
            {
                var task = protocol switch
                {
                    ProtocolTypes.Email when settings is EmailSettings emailSettings 
                        => SendEmail(emailSettings),
                    
                    ProtocolTypes.WebSocket when settings is WebSocketSettings webSocketSettings 
                        => //_notificationHub.Clients
                           // .User(BuildUserName(notification.Receiver, notification.UserId))
                           // .ReceiveMessage(entry.Entity.Id, notification.Message),
                           SendPrivateMessage(notification.Receiver, notification.UserId, entry.Entity.Id, notification.Message),
                    
                    _ => throw new ArgumentException($"Unsupported protocol '{protocol}' or incorrect settings type")
                };
                
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }


        public async Task MarkAsRead(int notificationId)
        {
            var notification = await _context.Notifications
                .SingleOrDefaultAsync(n => n.Id == notificationId && !n.IsRead);

            if (notification is not null)
            {
                notification.IsRead = true;
                _context.Notifications.Update(notification);
                await _context.SaveChangesAsync();
            }
        }


        public async Task<List<SlimNotification>> GetNotifications(ReceiverTypes receiver, int userId, int top, int skip)
        {
            return await _context.Notifications
                .Where(n => n.Receiver == receiver && n.UserId == userId)
                .Take(top)
                .Skip(skip)
                .Select(n => new SlimNotification
                {
                    Receiver = n.Receiver,
                    Id = n.Id,
                    UserId = n.UserId,
                    Message = n.Message,
                    Type = n.Type,
                    Created = n.Created,
                    IsRead = n.IsRead
                })
                .ToListAsync();
        }


        private Task SendEmail(EmailSettings settings)
        {
            // TODO: Sending e-mails will be implemented later in task AA-128
            return Task.CompletedTask;
        }


        private async Task SendPrivateMessage(ReceiverTypes receiver, int userId, int messageId, JsonDocument message)
        {
            await _notificationHub.Clients
                .Group(userId.ToString())
                //.User(BuildUserId(receiver, userId))
                .ReceiveMessage(messageId, message);
        }


        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;
        private readonly EdoContext _context;
    }
}