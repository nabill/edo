using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface ICounterpartyVerificationService
    {
        Task<Result> VerifyAsReadOnly(int counterpartyId, string reason);

        Task<Result> VerifyAsFullyAccessed(int counterpartyId, CounterpartyContractKind contractKind, string reason);

        Task<Result> DeclineVerification(int counterpartyId, string verificationReason);
    }
}