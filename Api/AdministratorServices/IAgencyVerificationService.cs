using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgencyVerificationService
    {
        Task<Result> VerifyAsFullyAccessed(int agencyId, AgencyFullAccessVerificationRequest request);
        Task<Result> VerifyAsReadOnly(int agencyId, string verificationReason);
        Task<Result> DeclineVerification(int agencyId, string verificationReason);
    }
}