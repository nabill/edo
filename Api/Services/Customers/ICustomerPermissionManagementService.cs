using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public interface ICustomerPermissionManagementService
    {
        Task<Result<List<InCounterpartyPermissions>>> SetInCounterpartyPermissions(
            int companyId, int branchId, int customerId, List<InCounterpartyPermissions> permissions);

        Task<Result<List<InCounterpartyPermissions>>> SetInCounterpartyPermissions(
            int companyId, int branchId, int customerId, InCounterpartyPermissions permissions);
    }
}