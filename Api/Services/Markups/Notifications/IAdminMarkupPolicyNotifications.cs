using Api.Models.Mailing;
using HappyTravel.Edo.Data.Markup;
using System.Threading.Tasks;

namespace Api.Services.Markups.Notifications
{
    public interface IAdminMarkupPolicyNotifications
    {
        Task NotifyMarkupAddedOrModified(MarkupPolicy policy, MarkupChangedData changedData);
    }
}