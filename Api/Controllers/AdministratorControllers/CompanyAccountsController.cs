using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Company;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin")]
    [Produces("application/json")]
    public class CompanyAccountsController : BaseController
    {
        public CompanyAccountsController(ICompanyAccountService companyAccountService)
        {
            _companyAccountService = companyAccountService;
        }


        /// <summary>
        ///     Gets company banks list
        /// </summary>
        [HttpGet("company/banks")]
        [ProducesResponseType(typeof(List<FullAgencyAccountInfo>), StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.CompanyAccountManagement)]
        public async Task<IActionResult> GetCompanyBanks() 
            => Ok(await _companyAccountService.GetAllBanks());
        
        
        /// <summary>
        ///     Adds a new company bank
        /// </summary>
        /// <param name="bankInfo">A new company bank info</param>
        [HttpPost("company/banks")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CompanyAccountManagement)]
        public async Task<IActionResult> Add([FromBody]CompanyBankInfo bankInfo)
            => NoContentOrBadRequest(await _companyAccountService.AddBank(bankInfo));


        /// <summary>
        ///     Modifies an existing company bank
        /// </summary>
        /// <param name="bankInfo">New info for the company bank</param>
        /// <param name="bankId">Id of the company bank to modify</param>
        [HttpPut("company/banks/{bankId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CompanyAccountManagement)]
        public async Task<IActionResult> ModifyBank([FromBody] CompanyBankInfo bankInfo, [FromRoute] int bankId)
            => NoContentOrBadRequest(await _companyAccountService.ModifyBank(bankId, bankInfo));


        /// <summary>
        ///     Removes a company bank
        /// </summary>
        /// <param name="bankId">Id of the company bank to delete</param>
        [HttpDelete("company/banks/{bankId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CompanyAccountManagement)]
        public async Task<IActionResult> RemoveBank([FromRoute] int bankId)
            => NoContentOrBadRequest(await _companyAccountService.RemoveBank(bankId));
        
        
        private readonly ICompanyAccountService _companyAccountService;
    }
}