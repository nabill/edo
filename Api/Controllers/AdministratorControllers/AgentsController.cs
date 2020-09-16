using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Data.Agents;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin")]
    [Produces("application/json")]
    public class AgentsController : BaseController
    {
        public AgentsController(IAgentSystemSettingsManagementService systemSettingsManagementService)
        {
            _systemSettingsManagementService = systemSettingsManagementService;
        }
        
        
        /// <summary>
        /// Updates agent's availability search settings
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="agentId">Agent Id</param>
        /// <param name="agencyId">Agency Id</param>
        /// <returns></returns>
        [HttpPut("agencies/{agencyId}/agents/{agentId}/system-settings/availability-search")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> SetSystemSettings([FromBody] AgentAvailabilitySearchSettings settings, [FromRoute] int agentId, [FromRoute] int agencyId)
        {
            var (_, isFailure, error) = await _systemSettingsManagementService.SetAvailabilitySearchSettings(agentId, agencyId, settings);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok();
        }
        
        /// <summary>
        /// Gets agent's availability search settings
        /// </summary>
        /// <param name="agentId">Agent Id</param>
        /// <param name="agencyId">Agency Id</param>
        /// <returns></returns>
        [HttpGet("agencies/{agencyId}/agents/{agentId}/system-settings/availability-search")]
        [ProducesResponseType(typeof(AgentAvailabilitySearchSettings), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> GetSystemSettings([FromRoute] int agentId, [FromRoute] int agencyId)
        {
            var (_, isFailure, settings, error) = await _systemSettingsManagementService.GetAvailabilitySearchSettings(agentId, agencyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(settings);
        }
        
        
        private readonly IAgentSystemSettingsManagementService _systemSettingsManagementService;
    }
}