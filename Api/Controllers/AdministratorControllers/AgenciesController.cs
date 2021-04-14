using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Bookings;
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
            IAdminAgencyManagementService agencyManagementService, IBookingService bookingService)
        {
            _systemSettingsManagementService = systemSettingsManagementService;
            _agentService = agentService;
            _agencyManagementService = agencyManagementService;
            _bookingService = bookingService;
        }


        /// <summary>
        ///     Gets a list of all bookings made by the agency
        /// </summary>
        /// <param name="agencyId">Agency Id</param>
        /// <returns>List of bookings</returns>
        [HttpGet("{agencyId}/accommodations/bookings")]
        [ProducesResponseType(typeof(List<Booking>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> GetAgencyBookings([FromRoute] int agencyId)
        {
            var (_, isFailure, bookings, error) = await _bookingService.GetAgencyBookings(agencyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(bookings);
        }


        /// <summary>
        ///     Gets setting which tells what payment methods to show for booking payment.
        /// </summary>
        /// <param name="agencyId">Id of an agency to get settings for</param>
        /// <returns>Displayed payment methods setting</returns>
        [HttpGet("{agencyId}/system-settings/displayed-payment-options")]
        [ProducesResponseType(typeof(DisplayedPaymentOptionsSettings), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> GetDisplayedPaymentOptions([FromRoute] int agencyId)
            => OkOrBadRequest(await _systemSettingsManagementService.GetDisplayedPaymentOptions(agencyId));


        /// <summary>
        ///     Gets agent's availability search settings
        /// </summary>
        /// <param name="agencyId">Agency Id</param>
        /// <returns>Agency availability search settings</returns>
        [HttpGet("{agencyId}/system-settings/availability-search")]
        [ProducesResponseType(typeof(AgencyAccommodationBookingSettings), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> GetSystemSettings([FromRoute] int agencyId)
        {
            var (_, isFailure, settings, error) = await _systemSettingsManagementService.GetAvailabilitySearchSettings(agencyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(settings);
        }


        /// <summary>
        ///     Sets setting which tells what payment methods to show for booking payment.
        /// </summary>
        /// <param name="settings">Settings to set</param>
        /// <param name="agencyId">Id of an agency to set settings for</param>
        /// <returns></returns>
        [HttpPut("{agencyId}/system-settings/displayed-payment-options")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> SetDisplayedPaymentOptions([FromBody] DisplayedPaymentOptionsSettings settings, [FromRoute] int agencyId)
            => NoContentOrBadRequest(await _systemSettingsManagementService.SetDisplayedPaymentOptions(agencyId, settings));


        /// <summary>
        ///     Updates agent's availability search settings
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="agencyId">Agency Id</param>
        /// <returns></returns>
        [HttpPut("{agencyId}/system-settings/availability-search")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> SetSystemSettings([FromBody] AgencyAccommodationBookingSettings settings, [FromRoute] int agencyId)
            => NoContentOrBadRequest(await _systemSettingsManagementService.SetAvailabilitySearchSettings(agencyId, settings));


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


        private readonly IAgencySystemSettingsManagementService _systemSettingsManagementService;
        private readonly IAgentService _agentService;
        private readonly IAdminAgencyManagementService _agencyManagementService;
        private readonly IBookingService _bookingService;
    }
}