using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class AgentRoleExtensions
    {
        public static AgentRoleInfo ToAgentRoleInfo(this AgentRole agentRole)
            => new AgentRoleInfo
            {
                Id = agentRole.Id,
                Name = agentRole.Name,
                Permissions = agentRole.Permissions.ToList()
            };


        public static AgentRole ToAgentRole(this AgentRoleInfo agentRoleInfo)
            => new AgentRole
            {
                Name = agentRoleInfo.Name,
                Permissions = agentRoleInfo.Permissions.ToFlags()
            };
    }
}
