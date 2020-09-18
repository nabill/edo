using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/agencies")]
    [Produces("application/json")]
    public class AgenciesController : BaseController
    {
        public AgenciesController(IAgencySystemSettingsManagementService systemSettingsManagementService)
        {
            _systemSettingsManagementService = systemSettingsManagementService;
        }
        
        
        /// <summary>
        /// Updates agent's availability search settings
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="agencyId">Agency Id</param>
        /// <returns></returns>
        [HttpPut("{agencyId}/system-settings/availability-search")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> SetSystemSettings([FromBody] AgencyAvailabilitySearchSettings settings, [FromRoute] int agencyId)
        {
            var (_, isFailure, error) = await _systemSettingsManagementService.SetAvailabilitySearchSettings(agencyId, settings);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok();
        }
        
        /// <summary>
        /// Gets agent's availability search settings
        /// </summary>
        /// <param name="agencyId">Agency Id</param>
        /// <returns>Agency availability search settings</returns>
        [HttpGet("{agencyId}/system-settings/availability-search")]
        [ProducesResponseType(typeof(AgencyAvailabilitySearchSettings), (int)HttpStatusCode.OK)]
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
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> SetDisplayedPaymentOptions([FromBody] DisplayedPaymentOptionsSettings settings, [FromRoute] int agencyId) =>
            OkOrBadRequest(await _systemSettingsManagementService.SetDisplayedPaymentOptions(settings, agencyId));
        


        /// <summary>
        ///     Gets setting which tells what payment methods to show for booking payment.
        /// </summary>
        /// <param name="agencyId">Id of an agency to get settings for</param>
        /// <returns>Displayed payment methods setting</returns>
        [HttpGet("{agencyId}/system-settings/displayed-payment-options")]
        [ProducesResponseType(typeof(DisplayedPaymentOptionsSettings), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> GetDisplayedPaymentOptions([FromRoute] int agencyId) =>
            OkOrBadRequest(await _systemSettingsManagementService.GetDisplayedPaymentOptions(agencyId));


        private readonly IAgencySystemSettingsManagementService _systemSettingsManagementService;
    }
}