using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Services.Management;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Filters.Authorization.ServiceAccountFilters
{
    public class ServiceAccountRequiredAuthorizationHandler : AuthorizationHandler<ServiceAccountRequiredAuthorizationRequirement>
    {
        public ServiceAccountRequiredAuthorizationHandler(IServiceAccountContext serviceAccountContext, ILogger<AgentRequiredAuthorizationHandler> logger)
        {
            _serviceAccountContext = serviceAccountContext;
            _logger = logger;
        }
        
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ServiceAccountRequiredAuthorizationRequirement requirement)
        {
            var (_, isFailure, account, error) = await _serviceAccountContext.GetCurrent();
            if (isFailure)
            {
                _logger.LogServiceAccountFailedToAuthorize(error);
                context.Fail();
            }
            else
            {
                _logger.LogServiceAccountAuthorized($"Service account '{account.ClientId}' authorized successfully");
                context.Succeed(requirement);
            }
        }
        
        private readonly IServiceAccountContext _serviceAccountContext;
        private readonly ILogger<AgentRequiredAuthorizationHandler> _logger;
    }
}