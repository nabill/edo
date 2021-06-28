using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Models;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgentRolesManagementService
    {
        Task<List<AgentRoleInfo>> GetAll();

        Task<Result> Add(AgentRoleInfo roleInfo);

        Task<Result> Edit(int roleId, AgentRoleInfo roleInfo);

        Task<Result> Delete(int roleId);
    }
}