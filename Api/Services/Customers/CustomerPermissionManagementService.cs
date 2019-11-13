using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
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


        public Task<Result> SetInCompanyPermissions(int customerId, List<InCompanyPermissions> permissions)
        {
            return GetCurrentCustomer()
                .OnSuccess(CheckCurrentCustomerPermissions)
                .OnSuccess(GetCustomerRelation)
                .OnSuccess(UpdatePermissions);

            async Task<Result<CustomerInfo>> GetCurrentCustomer() => await _customerContext.GetCustomerInfo();
            
            Result<CustomerInfo> CheckCurrentCustomerPermissions(CustomerInfo currentCustomer)
            {
                var (isFailure, _, error) = _permissionChecker
                    .CheckInCompanyPermission(currentCustomer, InCompanyPermissions.PermissionManagement);

                return isFailure
                    ? Result.Fail<CustomerInfo>(error)
                    : Result.Ok(currentCustomer);
            }

            async Task<Result<CustomerCompanyRelation>> GetCustomerRelation(CustomerInfo currentCustomer)
            {
                var relation = await _context.CustomerCompanyRelations
                    .SingleOrDefaultAsync(c => c.CustomerId == customerId && c.CompanyId == currentCustomer.CompanyId);

                return relation is null
                    ? Result.Fail<CustomerCompanyRelation>($"Could not find relation for customer id: '{customerId}' and company with id: '{currentCustomer.CompanyId}'")
                    : Result.Ok(relation);
            }

            async Task<Result> UpdatePermissions(CustomerCompanyRelation relation)
            {
                relation.InCompanyPermissions = permissions
                    .Aggregate((p, pNext) => p | pNext);;
                
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