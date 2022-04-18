using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Models.Markups.Agency;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/agencies/{agencyId}/markup-policy")]
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
        [HttpPut]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> SetPolicy([FromRoute] int agencyId, [FromBody] SetAgencyMarkupRequest settings)
        {
            var (_, isFailure, error) = await _policyManager.AddAgencyPolicy(agencyId, settings);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        /// Gets markup policies for the agency.
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<MarkupInfo>), StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> GetChildAgencyPolicies([FromRoute] int agencyId)
            => Ok(await _policyManager.GetForAgency(agencyId));


        /// <summary>
        ///     Deletes policy.
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> RemovePolicy([FromRoute] int agencyId)
        {
            var (_, isFailure, error) = await _policyManager.RemoveAgencyPolicy(agencyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        private readonly IAdminMarkupPolicyManager _policyManager;
    }
}