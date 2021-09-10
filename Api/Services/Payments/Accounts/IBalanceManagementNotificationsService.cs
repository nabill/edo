using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public interface IBalanceManagementNotificationsService
    {
        Task SendNotificationIfRequired(int agencyAccountId, decimal initialBalance, decimal resultingBalance);
    }
}