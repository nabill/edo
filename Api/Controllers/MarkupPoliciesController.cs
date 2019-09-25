using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Api.Services.Markups;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/markups")]
    [Produces("application/json")]
    public class MarkupPoliciesController : BaseController
    {
        public MarkupPoliciesController(IMarkupPolicyManagementService policyManagementService)
        {
            _policyManagementService = policyManagementService;
        }
        
        /// <summary>
        /// Creates markup policy.
        /// </summary>
        /// <param name="policyData">Policy data.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreatePolicy([FromBody]MarkupPolicyData policyData)
        {
            var (isFailure, _, error) = await _policyManagementService.AddPolicy(policyData);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        /// <summary>
        /// Deletes policy.
        /// </summary>
        /// <param name="policyId">Id of the policy to delete.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeletePolicy(int policyId)
        {
            var (isFailure, _, error) = await _policyManagementService.DeletePolicy(policyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        /// <summary>
        /// Updates policy settings.
        /// </summary>
        /// <param name="policyId">Id of the policy.</param>
        /// <param name="policySettings">Updated settings.</param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> UpdatePolicySettings(int policyId, [FromBody]MarkupPolicySettings policySettings)
        {
            var (isFailure, _, error) = await _policyManagementService.UpdatePolicy(policyId, policySettings);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        /// <summary>
        /// Gets all global policies.
        /// </summary>
        /// <returns>Global policies.</returns>
        [HttpGet("global")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> GetGlobalPolicies()
        {
            var (isFailure, _, policies, error) = await _policyManagementService.GetGlobalPolicies();
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(policies);
        }
        
        /// <summary>
        /// Gets all company policies.
        /// </summary>
        /// <param name="companyId">Id of the company.</param>
        /// <returns>Company policies.</returns>
        [HttpGet("company/{companyId}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> GetCompanyPolicies(int companyId)
        {
            var (isFailure, _, policies, error) = await _policyManagementService.GetCompanyPolicies(companyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(policies);
        }
        
        /// <summary>
        /// Gets customer policies.
        /// </summary>
        /// <param name="customerId">Id of the customer.</param>
        /// <returns>Customer policies.</returns>
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> GetCustomerPolicies(int customerId)
        {
            var (isFailure, _, policies, error) = await _policyManagementService.GetCustomerPolicies(customerId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(policies);
        }
        
        private readonly IMarkupPolicyManagementService _policyManagementService;
    }
}