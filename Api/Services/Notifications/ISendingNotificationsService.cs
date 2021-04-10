using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Notifications.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Notifications
{
    public interface ISendingNotificationsService
    {
        Task<Result> Send(SlimAgentContext agent, string message, NotificationTypes notificationType, string email = "", string templateId = "");
        Task<Result> Send(SlimAgentContext agent, string message, NotificationTypes notificationType, List<string> emails = null, string templateId = "");
    }
}
