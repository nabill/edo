using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IAgentMarkupPolicyManager
    {
        Task<Result> Add(MarkupPolicyData policyData, AgentContext agent);

        Task<Result> Remove(int policyId, AgentContext agent);

        Task<Result> Modify(int policyId, MarkupPolicySettings settings, AgentContext agent);

        Task<List<MarkupPolicyData>> Get(MarkupPolicyScope scope);
    }
}