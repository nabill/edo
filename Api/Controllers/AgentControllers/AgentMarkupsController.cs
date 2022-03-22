using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Models.Markups.Agent;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/agency/agents/{agentId}/markup-policy")]
    [Produces("application/json")]
    public class AgentMarkupsController : BaseController
    {
        public AgentMarkupsController(IAgentMarkupPolicyManager policyManager,
            IAgentContextService agentContext)
        {
            _policyManager = policyManager;
            _agentContext = agentContext;
        }


        /// <summary>
        /// Creates markup policy.
        /// </summary>
        /// <param name="agentId">Agent id</param>
        /// <param name="request">Set agent markup request</param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [InAgencyPermissions(InAgencyPermissions.MarkupManagement)]
        public async Task<IActionResult> SetPolicy([FromRoute] int agentId, [FromBody] SetAgentMarkupRequest request)
        {
            var agent = await _agentContext.GetAgent();

            var (_, isFailure, error) = await _policyManager.Set(agentId, request, agent);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Deletes policy.
        /// </summary>
        /// <param name="agentId">Agent id</param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [InAgencyPermissions(InAgencyPermissions.MarkupManagement)]
        public async Task<IActionResult> RemovePolicy([FromRoute]int agentId)
        {
            var agent = await _agentContext.GetAgent();

            var (_, isFailure, error) = await _policyManager.Remove(agentId, agent);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        /// Gets policies for specified scope.
        /// </summary>
        /// <param name="agentId">Agent id</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<MarkupInfo>), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetPolicies(int agentId)
        {
            var agent = (await _agentContext.GetAgent());
            return Ok(await _policyManager.Get(agentId, agent));
        }


        private readonly IAgentContextService _agentContext;
        private readonly IAgentMarkupPolicyManager _policyManager;
    }
}