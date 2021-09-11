using System.Threading.Tasks;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public interface IBalanceManagementNotificationsService
    {
        Task SendNotificationIfRequired(AgencyAccount account, MoneyAmount chargedAmount);
    }
}