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
        
        [HttpPost]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> CreateAdditionPolicy([FromBody]MarkupPolicyData policyData)
        {
            var (isFailure, _, error) = await _policyManagementService.AddPolicy(policyData);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        [HttpDelete("{id}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeletePolicy(int policyId)
        {
            var (isFailure, _, error) = await _policyManagementService.DeletePolicy(policyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        [HttpPatch("{id}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> UpdatePolicySettings(int policyId, [FromBody]MarkupPolicySettings policyData)
        {
            var (isFailure, _, error) = await _policyManagementService.UpdatePolicy(policyId, policyData);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        [HttpGet("global")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> GetGlobalPolicies()
        {
            var (isFailure, _, policies, error) = await _policyManagementService.GetGlobalPolicies();
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(policies);
        }
        
        [HttpGet("company/{companyId}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> GetCompanyPolicies(int companyId)
        {
            var (isFailure, _, policies, error) = await _policyManagementService.GetCompanyPolicies(companyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(policies);
        }
        
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