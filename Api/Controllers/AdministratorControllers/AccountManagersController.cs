using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models.Agencies;
using Api.Models.Management.Administrators;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Controllers;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin")]
    [Produces("application/json")]
    public class AccountManagersController : BaseController
    {
        public AccountManagersController(IAdministratorManagementService administratorManagementService)
        {
            _administratorManagementService = administratorManagementService;
        }


        /// <summary>
        ///     Gets all account managers
        /// </summary>
        /// <returns>List of account managers' info</returns>
        [HttpGet("account-managers")]
        [ProducesResponseType(typeof(List<AccountManager>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgencyManagement)]
        public async Task<IActionResult> Get()
            => Ok(await _administratorManagementService.GetAccountManagers());


        /// <summary>
        ///     Adds an account manager to the agency
        /// </summary>
        /// <returns></returns>
        [HttpPut("agencies/{agencyId}/account-manager")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgencyManagement)]
        public async Task<IActionResult> Add([FromRoute] int agencyId, [FromBody] AddAccountManagerRequest request)
            => NoContentOrBadRequest(await _administratorManagementService.AddAccountManager(agencyId, request));


        private readonly IAdministratorManagementService _administratorManagementService;
    }
}