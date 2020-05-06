using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Services.Agents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters
{
    public class AgentRequiredAuthorizationHandler : AuthorizationHandler<AgentRequiredAuthorizationRequirement>
    {
        public AgentRequiredAuthorizationHandler(IAgentContextInternal agentContextInternal, ILogger<AgentRequiredAuthorizationHandler> logger)
        {
            _agentContextInternal = agentContextInternal;
            _logger = logger;
        }


        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AgentRequiredAuthorizationRequirement requirement)
        {
            var (_, isFailure, _, error) = await _agentContextInternal.GetAgentInfo();
            if (isFailure)
            {
                _logger.LogAgentFailedToAuthorize(error);
                context.Fail();
            }
            else
            {
                context.Succeed(requirement);
            }
        }


        private readonly IAgentContextInternal _agentContextInternal;
        private readonly ILogger<AgentRequiredAuthorizationHandler> _logger;
    }
}