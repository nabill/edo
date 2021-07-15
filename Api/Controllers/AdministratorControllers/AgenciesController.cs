using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Models.Settings;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/agencies")]
    [Produces("application/json")]
    public class AgenciesController : BaseController
    {
        public AgenciesController(IAgencySystemSettingsManagementService systemSettingsManagementService,
            IAgentService agentService,
            IAdminAgencyManagementService agencyManagementService)
        {
            _systemSettingsManagementService = systemSettingsManagementService;
            _agentService = agentService;
            _agencyManagementService = agencyManagementService;
        }


        /// <summary>
        ///     Gets agency's availability search settings
        /// </summary>
        /// <param name="agencyId">Agency Id</param>
        /// <returns>Agency availability search settings</returns>
        [HttpGet("{agencyId}/system-settings/availability-search")]
        [ProducesResponseType(typeof(AgencyAccommodationBookingSettingsInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> GetSystemSettings([FromRoute] int agencyId)
        {
            var (_, isFailure, settings, error) = await _systemSettingsManagementService.GetAvailabilitySearchSettings(agencyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            if (settings == default)
                return NoContent();

            return Ok(settings.ToAgencyAccommodationBookingSettingsInfo());
        }


        /// <summary>
        ///     Updates agency's availability search settings
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="agencyId">Agency Id</param>
        /// <returns></returns>
        [HttpPut("{agencyId}/system-settings/availability-search")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> SetSystemSettings([FromBody] AgencyAccommodationBookingSettingsInfo settings, [FromRoute] int agencyId)
            => NoContentOrBadRequest(await _systemSettingsManagementService.SetAvailabilitySearchSettings(agencyId, settings));


        /// <summary>
        ///     Deletes agency's availability search settings
        /// </summary>
        /// <param name="agencyId">Agency Id</param>
        /// <returns></returns>
        [HttpDelete("{agencyId}/system-settings/availability-search")]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> DeleteSystemSettings([FromRoute] int agencyId)
            => NoContentOrBadRequest(await _systemSettingsManagementService.DeleteAvailabilitySearchSettings(agencyId));
        

        /// <summary>
        ///     Gets a list of agents in the agency
        /// </summary>
        /// <param name="agencyId">Agency Id</param>
        /// <returns>List of agents</returns>
        [HttpGet("{agencyId}/agents")]
        [ProducesResponseType(typeof(List<SlimAgentInfo>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> GetAgents([FromRoute] int agencyId)
        {
            var (_, isFailure, agents, error) = await _agentService.GetAgents(agencyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(agents);
        }


        /// <summary>
        ///     Gets child agencies.
        /// </summary>
        /// <returns></returns>
        [HttpGet("{agencyId}/child-agencies")]
        [ProducesResponseType(typeof(List<AgencyInfo>), (int) HttpStatusCode.OK)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyManagement)]
        public async Task<IActionResult> GetChildAgencies([FromRoute] int agencyId)
            => Ok(await _agencyManagementService.GetChildAgencies(agencyId, LanguageCode));


        /// <summary>
        ///  Deactivates specified agency.
        /// </summary>
        /// <param name="agencyId">Id of the agency.</param>
        /// <param name="request">Request data for deactivation.</param>
        /// <returns></returns>
        [HttpPost("{agencyId}/deactivate")]
        [ProducesResponseType(typeof(CounterpartyInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyManagement)]
        public async Task<IActionResult> DeactivateAgency(int agencyId, ActivityStatusChangeRequest request)
        {
            var (_, isFailure, error) = await _agencyManagementService.DeactivateAgency(agencyId, request.Reason);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///  Activates specified agency.
        /// </summary>
        /// <param name="agencyId">Id of the agency.</param>
        /// <param name="request">Request data for activation.</param>
        /// <returns></returns>
        [HttpPost("{agencyId}/activate")]
        [ProducesResponseType(typeof(CounterpartyInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyManagement)]
        public async Task<IActionResult> ActivateAgency(int agencyId, ActivityStatusChangeRequest request)
        {
            var (_, isFailure, error) = await _agencyManagementService.ActivateAgency(agencyId, request.Reason);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///  Gets specified agency.
        /// </summary>
        /// <param name="agencyId">Id of the agency.</param>
        [HttpGet("{agencyId}")]
        [ProducesResponseType(typeof(AgencyInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> GetAgency(int agencyId)
            => OkOrBadRequest(await _agencyManagementService.Get(agencyId, LanguageCode));


        private readonly IAgencySystemSettingsManagementService _systemSettingsManagementService;
        private readonly IAgentService _agentService;
        private readonly IAdminAgencyManagementService _agencyManagementService;
    }
}