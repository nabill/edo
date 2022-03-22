using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Company;
using HappyTravel.Edo.Api.Services.Company;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/company")]
    [Produces("application/json")]
    public class CompanyController : BaseController
    {
        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }


        /// <summary>
        /// Gets the company information.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(CompanyInfo), (int) HttpStatusCode.OK)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _companyService.GetCompanyInfo();

            return OkOrBadRequest(result);
        }


        private readonly ICompanyService _companyService;
    }
}