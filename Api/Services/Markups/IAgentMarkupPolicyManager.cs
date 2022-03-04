using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Markups.Agent;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IAgentMarkupPolicyManager
    {
        Task<Result> Set(int agentId, SetAgentMarkupRequest request, AgentContext agent);

        Task<Result> Remove(int agentId, AgentContext agent);

        Task<AgentMarkupInfo?> Get(int agentId, AgentContext agent);
    }
}