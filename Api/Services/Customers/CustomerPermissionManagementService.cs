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


        public Task<Result<List<InCounterpartyPermissions>>> SetInCounterpartyPermissions(int companyId, int branchId, int customerId,
            List<InCounterpartyPermissions> permissionsList) =>
            SetInCounterpartyPermissions(companyId, branchId, customerId, permissionsList.Aggregate((p1, p2) => p1 | p2));


        public async Task<Result<List<InCounterpartyPermissions>>> SetInCounterpartyPermissions(int companyId, int branchId, int customerId,
            InCounterpartyPermissions permissions)
        {
            var customer = await _customerContext.GetCustomer();

            return await CheckPermission()
                .OnSuccess(CheckCompanyAndBranch)
                .OnSuccess(GetRelation)
                .Ensure(IsPermissionManagementRightNotLost, "Cannot revoke last permission management rights")
                .OnSuccess(UpdatePermissions);

            Result CheckPermission()
            {
                if (!customer.InCounterpartyPermissions.HasFlag(InCounterpartyPermissions.PermissionManagementInBranch)
                    && !customer.InCounterpartyPermissions.HasFlag(InCounterpartyPermissions.PermissionManagementInCounterparty))
                    return Result.Fail("You have no acceptance to manage customers permissions");

                return Result.Ok();
            }

            Result CheckCompanyAndBranch()
            {
                if (customer.CounterpartyId != companyId)
                {
                    return Result.Fail("The customer isn't affiliated with the counterparty");
                }

                // TODO When branch system gets ierarchic, this needs to be changed so that customer can see customers/markups of his own branch and its subbranches
                if (!customer.InCounterpartyPermissions.HasFlag(InCounterpartyPermissions.PermissionManagementInCounterparty)
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
                        $"Could not find relation between the customer {customerId} and the counterparty {companyId}")
                    : Result.Ok(relation);
            }


            async Task<bool> IsPermissionManagementRightNotLost(CustomerCompanyRelation relation)
            {
                if (permissions.HasFlag(InCounterpartyPermissions.PermissionManagementInCounterparty))
                    return true;

                return (await _context.CustomerCompanyRelations
                        .Where(r => r.CompanyId == relation.CompanyId && r.CustomerId != relation.CustomerId)
                        .ToListAsync())
                    .Any(c => c.InCounterpartyPermissions.HasFlag(InCounterpartyPermissions.PermissionManagementInCounterparty));
            }


            async Task<List<InCounterpartyPermissions>> UpdatePermissions(CustomerCompanyRelation relation)
            {
                relation.InCounterpartyPermissions = permissions;

                _context.CustomerCompanyRelations.Update(relation);
                await _context.SaveChangesAsync();

                return relation.InCounterpartyPermissions.ToList();
            }
        }


        private readonly EdoContext _context;
        private readonly ICustomerContext _customerContext;
        private readonly IPermissionChecker _permissionChecker;
    }
}