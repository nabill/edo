using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Models.Markups.Agency;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/agency/child-agencies/{agencyId}/markup-policy")]
    [Produces("application/json")]
    public class ChildAgencyMarkupController : BaseController
    {
        public ChildAgencyMarkupController(IChildAgencyMarkupPolicyManager policyManager, IAgentContextService agentContext)
        {
            _policyManager = policyManager;
            _agentContext = agentContext;
        }


        /// <summary>
        /// Creates markup policy.
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        /// <param name="request">Markup settings</param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [InAgencyPermissions(InAgencyPermissions.MarkupManagement)]
        public async Task<IActionResult> SetPolicy([FromRoute] int agencyId, [FromBody] SetAgencyMarkupRequest request)
        {
            var agent = await _agentContext.GetAgent();

            var (_, isFailure, error) = await _policyManager.Set(agencyId, request, agent);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        /// Gets markup policies for a child agency.
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(AgencyMarkupInfo), (int)HttpStatusCode.OK)]
        [InAgencyPermissions(InAgencyPermissions.MarkupManagement)]
        public async Task<IActionResult> GetChildAgencyPolicy([FromRoute] int agencyId)
        {
            var agent = await _agentContext.GetAgent();
            
            return OkOrBadRequest(await _policyManager.Get(agencyId, agent));
        }


        /// <summary>
        ///     Deletes policy.
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [InAgencyPermissions(InAgencyPermissions.MarkupManagement)]
        public async Task<IActionResult> RemovePolicy([FromRoute]int agencyId)
        {
            var agent = await _agentContext.GetAgent();

            var (_, isFailure, error) = await _policyManager.Remove(agencyId, agent);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        private readonly IAgentContextService _agentContext;
        private readonly IChildAgencyMarkupPolicyManager _policyManager;
    }
}