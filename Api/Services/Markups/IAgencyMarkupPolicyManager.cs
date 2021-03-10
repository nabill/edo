using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IAgencyMarkupPolicyManager
    {
        Task<Result> Add(int agencyId, MarkupPolicySettings policyData, AgentContext agent);

        Task<Result> Remove(int agencyId, int policyId, AgentContext agent);

        Task<Result> Modify(int agencyId, int policyId, MarkupPolicySettings settings, AgentContext agent);

        Task<List<MarkupInfo>> Get(int agencyId);
    }
}