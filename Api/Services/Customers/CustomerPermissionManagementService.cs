using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
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


        public Task<Result<List<InCompanyPermissions>>> SetInCompanyPermissions(int companyId, int branchId, int customerId,
            List<InCompanyPermissions> permissionsList) =>
            SetInCompanyPermissions(companyId, branchId, customerId, permissionsList.Aggregate((p1, p2) => p1 | p2));


        public async Task<Result<List<InCompanyPermissions>>> SetInCompanyPermissions(int companyId, int branchId, int customerId,
            InCompanyPermissions permissions)
        {
            var customer = await _customerContext.GetCustomer();

            return await CheckPermission()
                .OnSuccess(CheckCompanyAndBranch)
                .OnSuccess(GetRelation)
                .Ensure(IsPermissionManagementRightNotLost, "Cannot revoke last permission management rights")
                .OnSuccess(UpdatePermissions);

            Result CheckPermission()
            {
                if (!customer.InCompanyPermissions.HasFlag(InCompanyPermissions.PermissionManagementInBranch)
                    && !customer.InCompanyPermissions.HasFlag(InCompanyPermissions.PermissionManagementInCompany))
                    return Result.Fail("You have no acceptance to manage customers permissions");

                return Result.Ok();
            }

            Result CheckCompanyAndBranch()
            {
                if (customer.CompanyId != companyId)
                {
                    return Result.Fail("The customer isn't affiliated with the company");
                }

                // TODO When branch system gets ierarchic, this needs to be changed so that customer can see customers/markups of his own branch and its subbranches
                if (!customer.InCompanyPermissions.HasFlag(InCompanyPermissions.PermissionManagementInCompany)
                    && customer.BranchId != branchId)
                {
                    return Result.Fail("The customer isn't affiliated with the branch");
                }
                
                return Result.Ok();
            }

            async Task<Result<CustomerCompanyRelation>> GetRelation()
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
                if (permissions.HasFlag(InCompanyPermissions.PermissionManagementInCompany))
                    return true;

                return (await _context.CustomerCompanyRelations
                        .Where(r => r.CompanyId == relation.CompanyId && r.CustomerId != relation.CustomerId)
                        .ToListAsync())
                    .Any(c => c.InCompanyPermissions.HasFlag(InCompanyPermissions.PermissionManagementInCompany));
            }


            async Task<List<InCompanyPermissions>> UpdatePermissions(CustomerCompanyRelation relation)
            {
                relation.InCompanyPermissions = permissions;

                _context.CustomerCompanyRelations.Update(relation);
                await _context.SaveChangesAsync();

                return relation.InCompanyPermissions.ToList();
            }
        }


        private readonly EdoContext _context;
        private readonly ICustomerContext _customerContext;
        private readonly IPermissionChecker _permissionChecker;
    }
}