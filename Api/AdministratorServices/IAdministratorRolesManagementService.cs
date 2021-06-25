using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAdministratorRolesManagementService
    {
        Task<List<AdministratorRoleInfo>> GetAll();

        Task<Result> Add(AdministratorRoleInfo roleInfo);

        Task<Result> Edit(int roleId, AdministratorRoleInfo roleInfo);

        Task<Result> Delete(int roleId);
    }
}