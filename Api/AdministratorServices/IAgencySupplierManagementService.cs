using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.AdministratorServices;

public interface IAgencySupplierManagementService
{
    Task<Result<Dictionary<string, bool>>> GetMaterializedSuppliers(int agencyId);
    Task<Result> SaveSuppliers(int agencyId, Dictionary<string, bool> enabledSuppliers);
    Dictionary<string, bool> GetIntersection(Dictionary<string, bool> rootAgencySuppliers, Dictionary<string, bool> childAgencySuppliers);
}