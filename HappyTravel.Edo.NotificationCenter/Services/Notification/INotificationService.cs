using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.NotificationCenter.Models;
using HappyTravel.Edo.Notifications.Enums;

namespace HappyTravel.Edo.NotificationCenter.Services.Notification
{
    public interface INotificationService
    {
        Task Add(Notifications.Models.Notification notification);
        Task MarkAsRead(int notificationId);
        Task<List<SlimNotification>> GetNotifications(ReceiverTypes receiver, int userId, int top, int skip);
    }
}