using System;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}")]
    [Produces("application/json")]
    public class AgentAgencyEnablementController : BaseController
    {
        public AgentAgencyEnablementController(IAgentAgencyEnablementService agentAgencyEnablementService,
            IAgentContextService agentContextService)
        {
            _agentAgencyEnablementService = agentAgencyEnablementService;
            _agentContextService = agentContextService;
        }


        /// <summary>
        ///     Enables a given agent to operate using a given agency
        /// </summary>
        [HttpPut("agencies/{agencyId}/agents/{agentId}/enable")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [InAgencyPermissions(InAgencyPermissions.AgentEnablement)]
        public async Task<IActionResult> Enable(int agencyId, int agentId)
        {
            var agentContext = await _agentContextService.GetAgent();
            var result = await _agentAgencyEnablementService.Enable(agencyId, agentId, agentContext);

            return OkOrBadRequest(result);
        }


        /// <summary>
        ///     Disables a given agent to operate using a given agency
        /// </summary>
        [HttpPut("agencies/{agencyId}/agents/{agentId}/disable")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [InAgencyPermissions(InAgencyPermissions.AgentEnablement)]
        public async Task<IActionResult> Disable(int agencyId, int agentId)
        {
            var agentContext = await _agentContextService.GetAgent();
            var result = await _agentAgencyEnablementService.Disable(agencyId, agentId, agentContext);

            return OkOrBadRequest(result);
        }


        private readonly IAgentAgencyEnablementService _agentAgencyEnablementService;
        private readonly IAgentContextService _agentContextService;
    }
}
