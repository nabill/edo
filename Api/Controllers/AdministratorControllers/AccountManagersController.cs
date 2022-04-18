using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Controllers;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Models.Management.Administrators;
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
        [ProducesResponseType(typeof(List<AdministratorInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AdministratorManagement)]
        public async Task<IActionResult> Get()
            => Ok(await _administratorManagementService.GetAccountManagers());


        /// <summary>
        ///     Add account manager to agency
        /// </summary>
        /// <returns></returns>
        [HttpGet("agencies/{agencyId}/account-manager")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AdministratorManagement)]
        public async Task<IActionResult> Add([FromRoute] int agencyId, [FromQuery] int? accountManagerId)
            => NoContentOrBadRequest(await _administratorManagementService.AddAccountManager(agencyId, accountManagerId));


        private readonly IAdministratorManagementService _administratorManagementService;
    }
}