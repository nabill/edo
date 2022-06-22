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
using Api.AdministratorServices;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin")]
    [Produces("application/json")]
    public class CompanyAccountsController : BaseController
    {
        public CompanyAccountsController(ICompanyAccountService companyAccountService, ICompanyInfoService companyInfoService)
        {
            _companyAccountService = companyAccountService;
            _companyInfoService = companyInfoService;
        }


        /// <summary>
        /// Gets the company information.
        /// </summary>
        /// <returns></returns>
        [HttpGet("company")]
        [ProducesResponseType(typeof(CompanyInfo), StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.CompanyAccountManagement)]
        public async Task<IActionResult> GetCompanyInfo()
            => OkOrBadRequest(await _companyInfoService.Get());


        /// <summary>
        /// Updates the company information.
        /// </summary>
        /// <returns></returns>
        [HttpPut("company")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AdministratorPermissions(AdministratorPermissions.CompanyAccountManagement)]
        public async Task<IActionResult> UpdateCompanyInfo(CompanyInfo companyInfo)
        {
            await _companyInfoService.Update(companyInfo);

            return NoContent();
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
        public async Task<IActionResult> AddBank([FromBody] CompanyBankInfo bankInfo)
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


        /// <summary>
        ///     Gets company accounts list for bank
        /// </summary>
        /// <param name="bankId">Id of the company bank</param>
        [HttpGet("company/banks/{bankId:int}/accounts")]
        [ProducesResponseType(typeof(List<CompanyAccountInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CompanyAccountManagement)]
        public async Task<IActionResult> GetCompanyAccounts([FromRoute] int bankId)
            => OkOrBadRequest(await _companyAccountService.GetAccounts(bankId));


        /// <summary>
        ///     Adds a new company account for bank
        /// </summary>
        /// <param name="bankId">Id of the company bank</param>
        /// <param name="accountInfo">A new company bank info</param>
        [HttpPost("company/banks/{bankId:int}/accounts")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CompanyAccountManagement)]
        public async Task<IActionResult> AddAccount([FromBody] CompanyAccountInfo accountInfo, [FromRoute] int bankId)
            => NoContentOrBadRequest(await _companyAccountService.AddAccount(bankId, accountInfo));


        /// <summary>
        ///     Removes company bank account
        /// </summary>
        /// <param name="bankId">Id of the company bank</param>
        /// <param name="accountId">Id of the company account to remove</param>
        [HttpDelete("company/banks/{bankId:int}/accounts/{accountId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CompanyAccountManagement)]
        public async Task<IActionResult> RemoveAccount([FromRoute] int bankId, [FromRoute] int accountId)
            => NoContentOrBadRequest(await _companyAccountService.RemoveAccount(bankId, accountId));


        /// <summary>
        ///     Modifies an existing company bank account
        /// </summary>
        /// <remarks>
        ///     Currency, IsDefault and CompanyBank modifications are ignored 
        /// </remarks>
        /// <param name="accountInfo">New info for the company account</param>
        /// <param name="bankId">Id of the company bank</param>
        /// <param name="accountId">Id of the company account to modify</param>
        [HttpPut("company/banks/{bankId:int}/accounts/{accountId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CompanyAccountManagement)]
        public async Task<IActionResult> ModifyAccount([FromBody] CompanyAccountInfo accountInfo, [FromRoute] int bankId, [FromRoute] int accountId)
            => NoContentOrBadRequest(await _companyAccountService.ModifyAccount(bankId, accountId, accountInfo));


        /// <summary>
        ///     Sets a company bank account as default
        /// </summary>
        /// <param name="bankId">Id of the company bank</param>
        /// <param name="accountId">Id of the company account to set default</param>
        [HttpPost("company/banks/{bankId:int}/accounts/{accountId:int}/set-default")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CompanyAccountManagement)]
        public async Task<IActionResult> SetAccountAsDefault([FromRoute] int bankId, [FromRoute] int accountId)
            => NoContentOrBadRequest(await _companyAccountService.SetAccountAsDefault(bankId, accountId));


        private readonly ICompanyAccountService _companyAccountService;
        private readonly ICompanyInfoService _companyInfoService;
    }
}