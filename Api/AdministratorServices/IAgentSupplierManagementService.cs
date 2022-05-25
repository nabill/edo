using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Api.AdministratorServices
{
    public interface IAgentSupplierManagementService
    {
        Task<Result<Dictionary<string, bool>>> GetMaterializedSuppliers(int agentId);
        Task<Result> SaveSuppliers(int agentId, Dictionary<string, bool> enabledSuppliers);
    }
}