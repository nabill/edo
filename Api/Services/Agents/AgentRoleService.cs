using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgentRoleService : IAgentRolesService
    {
        public AgentRoleService(EdoContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<AgentRoleInfo>> GetAll()
        {
            var agentRoles = await _context.AgentRoles.ToListAsync();
            return agentRoles.Select(ToAgentRoleInfo);
        }
        
        
        private static AgentRoleInfo ToAgentRoleInfo(AgentRole agentRole)
            => new (agentRole.Id, agentRole.Name, agentRole.Permissions.ToList());

        
        private readonly EdoContext _context;
    }
}