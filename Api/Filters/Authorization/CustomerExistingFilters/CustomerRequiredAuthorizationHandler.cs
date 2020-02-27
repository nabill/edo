using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Services.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Filters.Authorization.CustomerExistingFilters
{
    public class CustomerRequiredAuthorizationHandler : AuthorizationHandler<CustomerRequiredAuthorizationRequirement>
    {
        public CustomerRequiredAuthorizationHandler(ICustomerContextInternal customerContextInternal, ILogger<CustomerRequiredAuthorizationHandler> logger)
        {
            _customerContextInternal = customerContextInternal;
            _logger = logger;
        }


        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomerRequiredAuthorizationRequirement requirement)
        {
            var (_, isFailure, _, error) = await _customerContextInternal.GetCustomerInfo();
            if (isFailure)
            {
                _logger.LogCustomerFailedToAuthorize(error);
                context.Fail();
            }
            else
            {
                context.Succeed(requirement);
            }
        }


        private readonly ICustomerContextInternal _customerContextInternal;
        private readonly ILogger<CustomerRequiredAuthorizationHandler> _logger;
    }
}