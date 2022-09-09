using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentService
    {
        Task<Result<Agent>> Add(UserDescriptionInfo agentRegistration, string externalIdentity, string email, Currencies preferredCurrency);

        Task<Result<Agent>> GetMasterAgent(int agencyId);

        Task<UserDescriptionInfo> UpdateCurrentAgent(UserDescriptionInfo newInfo, AgentContext agentContext);

        IQueryable<SlimAgentInfo> GetAgents(AgentContext agentContext);

        Task<Result<AgentInfoInAgency>> GetAgent(int agentId, AgentContext agentContext);

        Task<List<AgentAgencyRelationInfo>> GetAgentRelations(AgentContext agent);
    }
}