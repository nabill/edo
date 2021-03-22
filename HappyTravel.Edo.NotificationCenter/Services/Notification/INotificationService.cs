using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums.Notifications;
using HappyTravel.Edo.NotificationCenter.Models;

namespace HappyTravel.Edo.NotificationCenter.Services.Notification
{
    public interface INotificationService
    {
        Task Add(Models.Notification notification);
        Task MarkAsRead(int notificationId);
        Task<List<SlimNotification>> GetNotifications(int userId, int top, int skip);
    }
}