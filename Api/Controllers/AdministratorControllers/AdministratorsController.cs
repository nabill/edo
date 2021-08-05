using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management.Administrators;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/administrators")]
    [Produces("application/json")]
    public class AdministratorsController : BaseController
    {
        public AdministratorsController(IAdministratorContext administratorContext,
            IAdministratorRolesAssignmentService administratorRolesAssignmentService,
            IAdministratorManagementService administratorManagementService,
            IAdministratorService administratorService)
        {
            _administratorContext = administratorContext;
            _administratorRolesAssignmentService = administratorRolesAssignmentService;
            _administratorManagementService = administratorManagementService;
            _administratorService = administratorService;
        }


        /// <summary>
        ///     Gets current administrator information
        /// </summary>
        /// <returns>Current administrator information.</returns>
        [HttpGet("current")]
        [ProducesResponseType(typeof(RichAdministratorInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetCurrent()
        {
            var (_, isFailure, administrator, _) = await _administratorService.GetCurrentWithPermissions();

            return isFailure
                ? NoContent()
                : Ok(administrator);
        }


        /// <summary>
        ///     Gets all administrators information
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<AdministratorInfo>), StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.AdministratorManagement)]
        public async Task<IActionResult> GetAll()
            => Ok(await _administratorManagementService.GetAll());


        /// <summary>
        ///     Assigns new set of roles to an administrator
        /// </summary>
        [HttpPut("{administratorId:int}/roles")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.AdministratorManagement)]
        public async Task<IActionResult> SetRoles([FromRoute] int administratorId, [FromBody] List<int> roleIds)
        {
            var (_, isGetAdminFailure, administrator, getAdminError) = await _administratorContext.GetCurrent();

            if (isGetAdminFailure)
                return BadRequest(ProblemDetailsBuilder.Build(getAdminError));

            return OkOrBadRequest(await _administratorRolesAssignmentService.SetAdministratorRoles(administratorId, roleIds, administrator));
        }


        /// <summary>
        ///     Activates an administrator
        /// </summary>
        [HttpPost("{administratorId:int}/activate")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.AdministratorManagement)]
        public async Task<IActionResult> Activate([FromRoute] int administratorId)
        {
            var (_, isGetAdminFailure, administrator, getAdminError) = await _administratorContext.GetCurrent();

            if (isGetAdminFailure)
                return BadRequest(ProblemDetailsBuilder.Build(getAdminError));

            return OkOrBadRequest(await _administratorManagementService.Activate(administratorId, administrator));
        }


        /// <summary>
        ///     Deactivates an administrator
        /// </summary>
        [HttpPost("{administratorId:int}/deactivate")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.AdministratorManagement)]
        public async Task<IActionResult> Deactivate([FromRoute] int administratorId)
        {
            var (_, isGetAdminFailure, administrator, getAdminError) = await _administratorContext.GetCurrent();

            if (isGetAdminFailure)
                return BadRequest(ProblemDetailsBuilder.Build(getAdminError));

            return OkOrBadRequest(await _administratorManagementService.Deactivate(administratorId, administrator));
        }


        private readonly IAdministratorContext _administratorContext;
        private readonly IAdministratorRolesAssignmentService _administratorRolesAssignmentService;
        private readonly IAdministratorManagementService _administratorManagementService;
        private readonly IAdministratorService _administratorService;
    }
}