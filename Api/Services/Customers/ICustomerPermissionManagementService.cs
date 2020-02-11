using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface ICustomerPermissionManagementService
    {
        Task<Result> SetInCompanyPermissions(int companyId, int branchId, int customerId, InCompanyPermissions permissions);

        Task<Result> SetInCompanyPermissions(int companyId, int branchId, int customerId, List<InCompanyPermissions> permissions);
    }
}