using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Notifications;
using HappyTravel.Edo.Common.Enums.Notifications;

namespace HappyTravel.Edo.Api.Services.Notifications
{
    public interface INotificationOptionsService
    {
        Task<Result<SlimNotificationOptions>> GetNotificationOptions(NotificationTypes type, AgentContext agent);

        Task<Result> Update(NotificationTypes type, SlimNotificationOptions option, AgentContext agentContext);
    }
}