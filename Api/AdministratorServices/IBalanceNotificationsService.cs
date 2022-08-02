using System.Threading;
using System.Threading.Tasks;

namespace Api.AdministratorServices
{
    public interface IBalanceNotificationsService
    {
        Task NotifyCreditLimitRunOutBalance(CancellationToken cancellationToken = default);
    }
}