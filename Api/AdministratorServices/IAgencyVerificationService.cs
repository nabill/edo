using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgencyVerificationService
    {
        Task<Result> VerifyAsFullyAccessed(int agencyId, CounterpartyContractKind contractKind, string verificationReason);
        Task<Result> VerifyAsReadOnly(int agencyId, string verificationReason);
        Task<Result> DeclineVerification(int agencyId, string verificationReason);
    }
}