using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentStatusManagementService
    {
        Task<Result> Enable(int agentIdToEnable);

        Task<Result> Disable(int agentIdToDisable);
    }
}
