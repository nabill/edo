using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Api.AdministratorServices
{
    public interface IBalanceNotificationsService
    {
        Task<Result> NotifyCreditLimitRunOutBalance(CancellationToken cancellationToken = default);
    }
}