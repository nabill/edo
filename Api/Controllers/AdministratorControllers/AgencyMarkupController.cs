using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin")]
    [Produces("application/json")]
    public class AgencyMarkupController : BaseController
    {
        public AgencyMarkupController(IAdminMarkupPolicyManager policyManager)
        {
            _policyManager = policyManager;
        }
        
        
        /// <summary>
        /// Creates markup policy.
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        /// <param name="settings">Markup settings</param>
        /// <returns></returns>
        [HttpPost("agencies/{agencyId}/markup-policies")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> AddPolicy([FromRoute] int agencyId, [FromBody] MarkupPolicySettings settings)
        {
            var (_, isFailure, error) = await _policyManager.AddAgencyPolicy(agencyId, settings);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        
        /// <summary>
        /// Gets markup policies for a child agency.
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        /// <returns></returns>
        [HttpGet("agencies/{agencyId}/markup-policies")]
        [ProducesResponseType(typeof(List<MarkupInfo>), StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> GetChildAgencyPolicies([FromRoute] int agencyId) 
            => Ok(await _policyManager.GetForAgency(agencyId));


        /// <summary>
        ///     Deletes policy.
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        /// <param name="policyId">Id of the policy to delete.</param>
        /// <returns></returns>
        [HttpDelete("agencies/{agencyId}/markup-policies/{policyId}")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> RemovePolicy([FromRoute]int agencyId, [FromRoute] int policyId)
        {
            var (_, isFailure, error) = await _policyManager.RemoveAgencyPolicy(agencyId, policyId);
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
        [HttpPut("agencies/{agencyId}/markup-policies/{policyId}")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> ModifyPolicy(int agencyId, int policyId, [FromBody] MarkupPolicySettings policySettings)
        {
            var (_, isFailure, error) = await _policyManager.ModifyForAgency(agencyId, policyId, policySettings);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        
        private readonly IAdminMarkupPolicyManager _policyManager;
    }
}