using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentRolesAssignmentService
    {
        Task<Result> SetInAgencyRoles(int agentId, List<int> roleIdsList, AgentContext agent);
    }
}