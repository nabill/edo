using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface ICounterpartyVerificationService
    {
        Task<Result> VerifyAsReadOnly(int counterpartyId, string reason);

        Task<Result> VerifyAsFullyAccessed(int counterpartyId, string reason);

        Task<Result> DeclineVerification(int counterpartyId, string verificationReason);
    }
}