using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.AdministratorServices;

public interface IAgencySupplierManagementService
{
    Task<Result<Dictionary<string, bool>>> GetMaterializedSuppliers(int agencyId);
}