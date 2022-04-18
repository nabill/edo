using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models.Management.Administrators;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Management.Administrators;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAdministratorManagementService
    {
        Task<List<AdministratorInfo>> GetAll();
        Task<Result> Activate(int administratorId, Administrator initiator);
        Task<Result> Deactivate(int administratorId, Administrator initiator);
        Task<List<AccountManager>> GetAccountManagers();
    }
}