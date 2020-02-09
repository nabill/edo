using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class CustomerPermissionManagementService : ICustomerPermissionManagementService
    {
        public CustomerPermissionManagementService(EdoContext context,
            ICustomerContext customerContext, IPermissionChecker permissionChecker)
        {
            _context = context;
            _customerContext = customerContext;
            _permissionChecker = permissionChecker;
        }


        public Task<Result> SetInCompanyPermissions(int companyId, int branchId, int customerId, List<InCompanyPermissions> permissions)
        {
            var ps = permissions.Aggregate((p, pNext) => p | pNext);
            return SetInCompanyPermissions(companyId, branchId, customerId, ps);
        }


        public Task<Result> SetInCompanyPermissions(int companyId, int branchId, int customerId, InCompanyPermissions permissions)
        {
            return GetCurrentCustomer()
                .OnSuccess(CheckCurrentCustomerCanChangePermissions)
                .OnSuccess(GetCompanyRelation)
                .Ensure(IsPermissionManagementRightNotLost, "Cannot revoke last permission management rights")
                .OnSuccess(UpdatePermissions);

            async Task<Result<CustomerInfo>> GetCurrentCustomer() => await _customerContext.GetCustomerInfo();


            async Task<Result<CustomerInfo>> CheckCurrentCustomerCanChangePermissions(CustomerInfo currentCustomer)
            {
                var (_, isFailure, error) = await _permissionChecker
                    .CheckInCompanyPermission(currentCustomer, InCompanyPermissions.PermissionManagement);

                return isFailure
                    ? Result.Fail<CustomerInfo>(error)
                    : Result.Ok(currentCustomer);
            }


            async Task<Result<CustomerCompanyRelation>> GetCompanyRelation(CustomerInfo currentCustomer)
            {
                var relation = await _context.CustomerCompanyRelations
                    .SingleOrDefaultAsync(r => r.CustomerId == customerId && r.CompanyId == companyId && r.BranchId == branchId);

                return relation is null
                    ? Result.Fail<CustomerCompanyRelation>(
                        $"Could not find relation between the customer {customerId} and the company {companyId}")
                    : Result.Ok(relation);
            }


            async Task<bool> IsPermissionManagementRightNotLost(CustomerCompanyRelation relation)
            {
                if (permissions.HasFlag(InCompanyPermissions.PermissionManagement))
                    return true;

                return (await _context.CustomerCompanyRelations
                        .Where(r => r.CompanyId == relation.CompanyId && r.CustomerId != relation.CustomerId)
                        .ToListAsync())
                    .Any(c => c.InCompanyPermissions.HasFlag(InCompanyPermissions.PermissionManagement));
            }


            async Task<Result> UpdatePermissions(CustomerCompanyRelation relation)
            {
                relation.InCompanyPermissions = permissions;

                _context.CustomerCompanyRelations.Update(relation);
                await _context.SaveChangesAsync();

                return Result.Ok();
            }
        }


        private readonly EdoContext _context;
        private readonly ICustomerContext _customerContext;
        private readonly IPermissionChecker _permissionChecker;
    }
}