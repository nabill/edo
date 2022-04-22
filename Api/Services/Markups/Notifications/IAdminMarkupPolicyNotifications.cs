using HappyTravel.Edo.Data.Markup;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Services.Markups.Notifications
{
    public interface IAdminMarkupPolicyNotifications
    {
        Task NotifyMarkupAddedOrModified(MarkupPolicy newPolicy, MarkupPolicy? oldPolicy);
    }
}