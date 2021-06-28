using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/agent-roles")]
    [Produces("application/json")]
    public class AgentRolesController : BaseController
    {
        public AgentRolesController(IAgentRolesService agentRolesService)
        {
            _agentRolesService = agentRolesService;
        }


        /// <summary>
        ///     Gets all pssible agent roles
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<AgentRoleInfo>), StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> GetAll()
            => Ok(await _agentRolesService.GetAllRoles());


        /// <summary>
        ///     Adds a new agent role
        /// </summary>
        /// <param name="roleInfo">A new role info</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> Add([FromBody] AgentRoleInfo roleInfo)
            => OkOrBadRequest(await _agentRolesService.Add(roleInfo));


        /// <summary>
        ///     Edits an existing role
        /// </summary>
        /// <param name="roleInfo">New info for the role</param>
        /// <param name="roleId"></param>
        [HttpPut("{roleId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> Edit([FromBody] AgentRoleInfo roleInfo, [FromRoute] int roleId)
            => OkOrBadRequest(await _agentRolesService.Edit(roleId, roleInfo));


        /// <summary>
        ///     Deletes a role
        /// </summary>
        /// <param name="roleId">Id of the role to delete</param>
        [HttpDelete("{roleId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> Delete([FromRoute] int roleId)
            => OkOrBadRequest(await _agentRolesService.Delete(roleId));


        private readonly IAgentRolesService _agentRolesService;
    }
}