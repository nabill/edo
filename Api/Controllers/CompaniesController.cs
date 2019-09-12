using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Services.Customers;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/companies")]
    [Produces("application/json")]
    public class CompaniesController : BaseController
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
        }
        
        /// <summary>
        ///     Set company verified.
        /// </summary>
        /// <param name="companyId">Id of the company to verify.</param>
        /// <param name="request">Verification details.</param>
        /// <returns></returns>
        [HttpPost("verify/{companyId}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SetCompanyVerified(int companyId, [FromBody]CompanyVerificationRequest request)
        {
            var (isSuccess, _, error) = await _companyService.SetVerified(companyId, request.Reason);

            return isSuccess
                ? (IActionResult)NoContent()
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }
    }
}