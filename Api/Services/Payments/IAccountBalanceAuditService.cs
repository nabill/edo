using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface IAccountBalanceAuditService
    {
        Task Write<TEventData>(AccountEventType eventType, int accountId, decimal amount, int userEntityId, UserType userType, TEventData eventData);
    }
}
