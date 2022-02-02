using System;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/administrator-roles")]
    [Produces("application/json")]
    public class AdministratorRolesController : BaseController
    {
        public AdministratorRolesController(IAdministratorRolesManagementService administratorRolesManagementService)
        {
            _administratorRolesManagementService = administratorRolesManagementService;
        }


        /// <summary>
        ///     Gets all possible administrator roles
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<AdministratorRoleInfo>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
            => Ok(await _administratorRolesManagementService.GetAll());
        
        
        /// <summary>
        ///     Gets all possible administrator permissions
        /// </summary>
        /// <returns> Array of all permission names </returns>
        [HttpGet("permissions")]
        [ProducesResponseType(typeof(IEnumerable<AdministratorPermissions>), (int) HttpStatusCode.OK)]
        public IActionResult GetAllPermissionsList() => Ok(Enum.GetValues<AdministratorPermissions>().ToList());


        /// <summary>
        ///     Adds a new administrator role
        /// </summary>
        /// <param name="roleInfo">A new role info</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AdministratorManagement)]
        public async Task<IActionResult> Add([FromBody] AdministratorRoleInfo roleInfo)
            => OkOrBadRequest(await _administratorRolesManagementService.Add(roleInfo));


        /// <summary>
        ///     Edits an existing administrator role
        /// </summary>
        /// <param name="roleInfo">New info for the role</param>
        /// <param name="roleId"></param>
        [HttpPut("{roleId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AdministratorManagement)]
        public async Task<IActionResult> Edit([FromBody] AdministratorRoleInfo roleInfo, [FromRoute] int roleId)
            => OkOrBadRequest(await _administratorRolesManagementService.Edit(roleId, roleInfo));


        /// <summary>
        ///     Deletes an administrator role
        /// </summary>
        /// <param name="roleId">Id of the role to delete</param>
        [HttpDelete("{roleId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AdministratorManagement)]
        public async Task<IActionResult> Delete([FromRoute] int roleId)
            => OkOrBadRequest(await _administratorRolesManagementService.Delete(roleId));


        private readonly IAdministratorRolesManagementService _administratorRolesManagementService;
    }
}
