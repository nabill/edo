using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.NotificationCenter.Models;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.Notifications.Models;

namespace HappyTravel.Edo.Api.NotificationCenter.Services
{
    public interface INotificationOptionsService
    {
        Task<Result<SlimNotificationOptions>> GetNotificationOptions(int userId, ApiCallerTypes userType, int? agencyId, NotificationTypes notificationType);
        Task<Result<List<RecipientWithNotificationOptions>>> GetNotificationOptions(Dictionary<int, string> recipients, NotificationTypes notificationType);

        Task<Dictionary<NotificationTypes, NotificationSettings>> Get(SlimAgentContext agent);
        Task<Dictionary<NotificationTypes, NotificationSettings>> Get(SlimAdminContext admin);

        Task<Result> Update(SlimAgentContext agent, Dictionary<NotificationTypes, NotificationSettings> notificationOptions);
        Task<Result> Update(SlimAdminContext admin, Dictionary<NotificationTypes, NotificationSettings> notificationOptions);
    }
}