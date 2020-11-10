using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentAgencyEnablementService
    {
        Task<Result> Enable(int agencyId, int agentIdToEnable, AgentContext agentContext);

        Task<Result> Disable(int agencyId, int agentIdToDisable, AgentContext agentContext);
    }
}
