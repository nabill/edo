using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.NotificationCenter.Models;
using HappyTravel.Edo.Notifications.Enums;

namespace HappyTravel.Edo.Api.NotificationCenter.Services
{
    public interface IInternalNotificationService
    {
        Task AddAdminNotification(int adminId, JsonDocument message, NotificationTypes notificationType, Dictionary<ProtocolTypes, object> sendingSettings);
        Task Add(Notifications.Models.Notification notification);
        Task MarkAsRead(int notificationId);
        Task<List<SlimNotification>> GetNotifications(ReceiverTypes receiver, int userId, int top, int skip);
    }
}