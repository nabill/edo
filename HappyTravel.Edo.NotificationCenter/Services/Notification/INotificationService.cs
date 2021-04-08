using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.NotificationCenter.Models;

namespace HappyTravel.Edo.NotificationCenter.Services.Notification
{
    public interface INotificationService
    {
        Task Add(Notifications.Models.Notification notification);
        Task MarkAsRead(int notificationId);
        Task<List<SlimNotification>> GetNotifications(int userId, int top, int skip);
    }
}