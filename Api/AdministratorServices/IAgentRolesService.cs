using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Models;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgentRolesService
    {
        Task<List<AgentRoleInfo>> GetAllRoles();

        Task<Result> Add(AgentRoleInfo roleInfo);

        Task<Result> Edit(int roleId, AgentRoleInfo roleInfo);

        Task<Result> Delete(int roleId);
    }
}