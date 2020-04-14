using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Services.Agents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Filters.Authorization.InCounterpartyPermissionFilters
{
    public class InCounterpartyPermissionAuthorizationHandler : AuthorizationHandler<InCounterpartyPermissionsAuthorizationRequirement>
    {
        public InCounterpartyPermissionAuthorizationHandler(IAgentContextInternal agentContextInternal,
            IPermissionChecker permissionChecker,
            ILogger<InCounterpartyPermissionAuthorizationHandler> logger)
        {
            _agentContextInternal = agentContextInternal;
            _permissionChecker = permissionChecker;
            _logger = logger;
        }


        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, InCounterpartyPermissionsAuthorizationRequirement requirement)
        {
            var (_, isAgentFailure, agent, agentError) = await _agentContextInternal.GetAgentInfo();
            if (isAgentFailure)
            {
                _logger.LogAgentFailedToAuthorize($"Could not find agent: '{agentError}'");
                context.Fail();
                return;
            }

            var (_, isPermissionFailure, permissionError) = await _permissionChecker.CheckInCounterpartyPermission(agent, requirement.Permissions);
            if (isPermissionFailure)
            {
                _logger.LogAgentFailedToAuthorize($"Permission denied: '{permissionError}'");
                context.Fail();
                return;
            }

            _logger.LogAgentAuthorized($"Successfully authorized agent '{agent.Email}' for '{requirement.Permissions}'");
            context.Succeed(requirement);
        }


        private readonly IAgentContextInternal _agentContextInternal;
        private readonly ILogger<InCounterpartyPermissionAuthorizationHandler> _logger;
        private readonly IPermissionChecker _permissionChecker;
    }
}