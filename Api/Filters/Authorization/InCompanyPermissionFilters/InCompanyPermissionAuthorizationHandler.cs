using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Filters.Authorization.InCompanyPermissionFilters
{
    public class InCompanyPermissionAuthorizationHandler : AuthorizationHandler<InCompanyPermissionsAuthorizationRequirement>
    {
        public InCompanyPermissionAuthorizationHandler(ICustomerContext customerContext,
            IPermissionChecker permissionChecker,
            ILogger<InCompanyPermissionAuthorizationHandler> logger)
        {
            _customerContext = customerContext;
            _permissionChecker = permissionChecker;
            _logger = logger;
        }


        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, InCompanyPermissionsAuthorizationRequirement requirement)
        {
            var (_, isCustomerFailure, customer, customerError) = await _customerContext.GetCustomerInfo();
            if (isCustomerFailure)
            {
                // TODO: log customer not found
                context.Fail();
                return;
            }

            var (_, isPermissionFailure, permissionError) = await _permissionChecker.CheckInCompanyPermission(customer, requirement.Permissions);
            if (isPermissionFailure)
            {
                // TODO: log permission not found
                context.Fail();
                return;
            }

            context.Succeed(requirement);
        }


        private readonly ICustomerContext _customerContext;
        private readonly ILogger<InCompanyPermissionAuthorizationHandler> _logger;
        private readonly IPermissionChecker _permissionChecker;
    }
}