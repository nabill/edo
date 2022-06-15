using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.SupplierOptionsClient.Models;

namespace HappyTravel.Edo.Api.AdministratorServices;

public interface IAgencySupplierManagementService
{
    Task<Result<Dictionary<string, bool>>> GetMaterializedSuppliers(int agencyId);
    Result<Dictionary<string, bool>> GetMaterializedSuppliers(Dictionary<string, bool>? agencySuppliers, Dictionary<string, bool>? rootSuppliers);
    Task<Result> SaveSuppliers(int agencyId, Dictionary<string, bool> enabledSuppliers);
    Dictionary<string, bool> GetIntersection(Dictionary<string, bool> rootAgencySuppliers, Dictionary<string, bool> childAgencySuppliers);
    Dictionary<string, bool> SunpuMaterialization(Dictionary<string, bool> suppliers, Dictionary<string, EnablementState> enabledSuppliers, bool withTestOnly);
}