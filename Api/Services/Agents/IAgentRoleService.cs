using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentRolesService
    {
        Task<IEnumerable<AgentRoleInfo>> GetAll();
        Task<int[]> GetAllRoleIds();
    } 
}