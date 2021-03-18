using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.NotificationCenter.Models;

namespace HappyTravel.Edo.NotificationCenter.Services.Message
{
    public interface INotificationService
    {
        Task Add(Notification notification);
        Task MarkAsRead(int notificationId);
        Task<List<NotificationSlim>> GetMessages(int userId, int top, int skip);
    }
}