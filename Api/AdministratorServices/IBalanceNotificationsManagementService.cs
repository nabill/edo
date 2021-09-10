using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IBalanceNotificationsManagementService
    {
        Task<Result<BalanceNotificationSettingInfo>> Get(int agencyAccountId);
        Task<Result> Set(int agencyAccountId, int[] thresholds);
    }
}