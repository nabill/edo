using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Markups.Global;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/global-markup")]
    [Produces("application/json")]
    public class GlobalMarkupController : BaseController
    {
        public GlobalMarkupController(IAdminMarkupPolicyManager policyManager)
        {
            _policyManager = policyManager;
        }
        
        
        /// <summary>
        /// Gets global markup policy
        /// </summary>
        /// <returns>Global markup policy</returns>
        [HttpGet]
        [ProducesResponseType(typeof(GlobalMarkupInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> Get()
        {
            return Ok(await _policyManager.GetGlobalPolicy());
        }
        
        
        /// <summary>
        ///     Deletes global policy.
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> RemovePolicy()
        {
            var (_, isFailure, error) = await _policyManager.RemoveGlobalPolicy();
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        
        /// <summary>
        ///     Updates global policy settings.
        /// </summary>
        /// <param name="policySettings">Updated settings.</param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> SetPolicy([FromBody] SetGlobalMarkupRequest policySettings)
        {
            var (_, isFailure, error) = await _policyManager.SetGlobalPolicy(policySettings);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        
        private readonly IAdminMarkupPolicyManager _policyManager;
    }
}