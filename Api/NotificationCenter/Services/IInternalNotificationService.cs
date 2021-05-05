using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.NotificationCenter.Models;
using HappyTravel.Edo.Notifications.Enums;

namespace HappyTravel.Edo.Api.NotificationCenter.Services
{
    public interface IInternalNotificationService
    {
        Task AddAdminNotification(int adminId, JsonDocument message, NotificationTypes notificationType, Dictionary<ProtocolTypes, object> sendingSettings);
        Task AddAdminNotification(int adminId, DataWithCompanyInfo messageData, NotificationTypes notificationType, Dictionary<ProtocolTypes, object> sendingSettings);

        Task AddAgentNotification(SlimAgentContext agent, JsonDocument message, NotificationTypes notificationType, Dictionary<ProtocolTypes, object> sendingSettings);
        Task AddAgentNotification(SlimAgentContext agent, DataWithCompanyInfo messageData, NotificationTypes notificationType, Dictionary<ProtocolTypes, object> sendingSettings);

        Task ChangeSendingStatus(int notificationId, SendingStatuses sendingStatus, DateTime changeTime);

        Task<List<SlimNotification>> GetNotifications(ReceiverTypes receiver, int userId, int? agencyId, int top, int skip);
    }
}