using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Services.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Filters.Authorization.InCounterpartyPermissionFilters
{
    public class InCounterpartyPermissionAuthorizationHandler : AuthorizationHandler<InCounterpartyPermissionsAuthorizationRequirement>
    {
        public InCounterpartyPermissionAuthorizationHandler(ICustomerContextInternal customerContextInternal,
            IPermissionChecker permissionChecker,
            ILogger<InCounterpartyPermissionAuthorizationHandler> logger)
        {
            _customerContextInternal = customerContextInternal;
            _permissionChecker = permissionChecker;
            _logger = logger;
        }


        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, InCounterpartyPermissionsAuthorizationRequirement requirement)
        {
            var (_, isCustomerFailure, customer, customerError) = await _customerContextInternal.GetCustomerInfo();
            if (isCustomerFailure)
            {
                _logger.LogCustomerFailedToAuthorize($"Could not find customer: '{customerError}'");
                context.Fail();
                return;
            }

            var (_, isPermissionFailure, permissionError) = await _permissionChecker.CheckInCounterpartyPermission(customer, requirement.Permissions);
            if (isPermissionFailure)
            {
                _logger.LogCustomerFailedToAuthorize($"Permission denied: '{permissionError}'");
                context.Fail();
                return;
            }

            _logger.LogCustomerAuthorized($"Successfully authorized customer '{customer.Email}' for '{requirement.Permissions}'");
            context.Succeed(requirement);
        }


        private readonly ICustomerContextInternal _customerContextInternal;
        private readonly ILogger<InCounterpartyPermissionAuthorizationHandler> _logger;
        private readonly IPermissionChecker _permissionChecker;
    }
}