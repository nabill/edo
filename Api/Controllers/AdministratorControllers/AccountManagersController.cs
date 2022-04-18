using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Controllers;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Models.Management.Administrators;
using HappyTravel.Edo.Common.Enums.Administrators;
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
        [ProducesResponseType(typeof(List<AdministratorInfo>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AdministratorManagement)]
        public async Task<IActionResult> Get()
            => Ok(await _administratorManagementService.GetAccountManagers());


        private readonly IAdministratorManagementService _administratorManagementService;
    }
}