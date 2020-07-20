using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Users;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentContextService
    {
        ValueTask<AgentContext> GetAgent();

        Task<List<AgentAgencyInfo>> GetAgentCounterparties();

        Task<bool> IsAgentAffiliatedWithCounterparty(int agentId, int counterpartyId);

        Task<bool> IsAgentAffiliatedWithAgency(int agentId, int agencyId);
    }
}