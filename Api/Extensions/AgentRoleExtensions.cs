using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class AgentRoleExtensions
    {
        public static AgentRoleInfo ToAgentRoleInfo(this AgentRole agentRole) 
            => new AgentRoleInfo(agentRole.Id, agentRole.Name, agentRole.Permissions);
    }
}