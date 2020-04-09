using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentService
    {
        Task<Result<Agent>> Add(AgentEditableInfo agentRegistration, string externalIdentity, string email);

        Task<Result<Agent>> GetMasterAgent(int counterpartyId);

        Task<AgentEditableInfo> UpdateCurrentAgent(AgentEditableInfo newInfo);

        Task<Result<List<SlimAgentInfo>>> GetAgents(int counterpartyId, int agencyId = default);

        Task<Result<AgentInfoInAgency>> GetAgent(int counterpartyId, int agencyId, int agentId);
    }
}