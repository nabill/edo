using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}")]
    [Produces("application/json")]
    public class AgencyMarkupController : BaseController
    {
        public AgencyMarkupController(IAgencyMarkupPolicyManager policyManager, IAgentContextService agentContext)
        {
            _policyManager = policyManager;
            _agentContext = agentContext;
        }


        /// <summary>
        /// Creates markup policy.
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        /// <param name="settings">Markup settings</param>
        /// <returns></returns>
        [HttpPost("agency/child-agencies/{agencyId}/markup-policies")]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [InAgencyPermissions(InAgencyPermissions.MarkupManagement)]
        public async Task<IActionResult> AddPolicy([FromRoute] int agencyId, [FromBody] MarkupPolicySettings settings)
        {
            var agent = await _agentContext.GetAgent();

            var (_, isFailure, error) = await _policyManager.Add(agencyId, settings, agent);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        /// Gets markup policies for a child agency.
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        /// <returns></returns>
        [HttpGet("agency/child-agencies/{agencyId}/markup-policies")]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(List<MarkupInfo>), (int)HttpStatusCode.OK)]
        [InAgencyPermissions(InAgencyPermissions.MarkupManagement)]
        public async Task<IActionResult> GetChildAgencyPolicies([FromRoute] int agencyId)
        {
            var agent = await _agentContext.GetAgent();

            var (_, isFailure, markupList, error) = await _policyManager.GetForChildAgency(agencyId, agent);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));
            
            return Ok(markupList);
        }


        /// <summary>
        ///     Deletes policy.
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        /// <param name="policyId">Id of the policy to delete.</param>
        /// <returns></returns>
        [HttpDelete("agency/child-agencies/{agencyId}/markup-policies/{policyId}")]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [InAgencyPermissions(InAgencyPermissions.MarkupManagement)]
        public async Task<IActionResult> RemovePolicy([FromRoute]int agencyId, [FromRoute] int policyId)
        {
            var agent = await _agentContext.GetAgent();

            var (_, isFailure, error) = await _policyManager.Remove(agencyId, policyId, agent);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Updates policy settings.
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        /// <param name="policyId">Id of the policy.</param>
        /// <param name="policySettings">Updated settings.</param>
        /// <returns></returns>
        [HttpPut("agency/child-agencies/{agencyId}/markup-policies/{policyId}")]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [InAgencyPermissions(InAgencyPermissions.MarkupManagement)]
        public async Task<IActionResult> ModifyPolicy(int agencyId, int policyId, [FromBody] MarkupPolicySettings policySettings)
        {
            var agent = await _agentContext.GetAgent();

            var (_, isFailure, error) = await _policyManager.Modify(agencyId, policyId, policySettings, agent);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        
        private readonly IAgentContextService _agentContext;
        private readonly IAgencyMarkupPolicyManager _policyManager;
    }
}