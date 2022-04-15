using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Models.Markups.Agency;
using HappyTravel.Edo.Api.Models.Markups.Global;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IAdminMarkupPolicyManager
    {
        Task<GlobalMarkupInfo?> GetGlobalPolicy();

        Task<Result> RemoveGlobalPolicy();

        Task<Result> AddGlobalPolicy(SetGlobalMarkupRequest settings);

        Task<List<MarkupInfo>> GetLocationPolicies();

        Task<Result> AddLocationPolicy(MarkupPolicySettings settings);

        Task<Result> RemoveLocationPolicy(int policyId);

        Task<Result> ModifyLocationPolicy(int policyId, MarkupPolicySettings settings);

        Task<Result> AddAgencyPolicy(int agencyId, SetAgencyMarkupRequest request);

        Task<AgencyMarkupInfo?> GetForAgency(int agencyId);

        Task<Result> RemoveAgencyPolicy(int agencyId);
    }
}