using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Filters.Authorization.AgencyVerificationStatesFilters
{
    public class MinAgencyVerificationStateAuthorizationHandler : AuthorizationHandler<MinAgencyVerificationStateAuthorizationRequirement>
    {
        public MinAgencyVerificationStateAuthorizationHandler(IAgentContextInternal agentContextInternal, IDoubleFlow flow,
            EdoContext context, ILogger<MinAgencyVerificationStateAuthorizationHandler> logger)
        {
            _agentContextInternal = agentContextInternal;
            _flow = flow;
            _context = context;
            _logger = logger;
        }


        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, MinAgencyVerificationStateAuthorizationRequirement requirement)
        {
            var (_, isAgentFailure, agent, agentError) = await _agentContextInternal.GetAgentInfo();
            if (isAgentFailure)
            {
                _logger.LogAgentAuthorizationFailure($"Could not find agent: '{agentError}'");
                context.Fail();
                return;
            }
            
            var rootAgencyState = await GetRootAgencyState(agent.AgencyId);

            switch (rootAgencyState)
            {
                case AgencyVerificationStates.FullAccess:
                    context.Succeed(requirement);
                    _logger.LogAgencyVerificationStateAuthorizationSuccess(agent.Email);
                    return;
                
                case AgencyVerificationStates.ReadOnly:
                    if (requirement.AgencyVerificationState == AgencyVerificationStates.ReadOnly)
                    {
                        context.Succeed(requirement);
                        _logger.LogAgencyVerificationStateAuthorizationSuccess(agent.Email);
                    }
                    else
                    {
                        _logger.LogAgencyVerificationStateAuthorizationFailure(agent.Email, rootAgencyState);
                        context.Fail();
                    }

                    return;

                default:
                    _logger.LogAgencyVerificationStateAuthorizationFailure(agent.Email, rootAgencyState);
                    context.Fail();
                    return;
            }


            Task<AgencyVerificationStates> GetRootAgencyState(int agencyId)
            {
                var cacheKey = _flow.BuildKey(nameof(MinAgencyVerificationStateAuthorizationHandler), nameof(GetRootAgencyState), agencyId.ToString());
                return _flow.GetOrSetAsync(cacheKey, ()
                        => _context.Agencies
                            .Where(a => a.Id == agencyId)
                            .Join(_context.Agencies,
                                a => a.Ancestors.Any()
                                    ? a.Ancestors[0]
                                    : a.Id,
                                ra => ra.Id,
                                (agency, rootAgency) => rootAgency)
                            .Select(ra => ra.VerificationState)
                            .SingleOrDefaultAsync(),
                    AgencyVerificationStateCacheTtl);
            }
        }


        private static readonly TimeSpan AgencyVerificationStateCacheTtl = TimeSpan.FromMinutes(5);
        private readonly EdoContext _context;
        private readonly ILogger<MinAgencyVerificationStateAuthorizationHandler> _logger;
        private readonly IAgentContextInternal _agentContextInternal;
        private readonly IDoubleFlow _flow;
    }
}