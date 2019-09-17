using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Accounts
{
    public interface IAccountAuditService
    {
        Task<Result> Write<TEventData>(AccountEventType eventType, TEventData eventData);
    }
}
