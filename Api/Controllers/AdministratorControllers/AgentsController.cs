using System.Collections.Generic;
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
    [Route("api/{v:apiVersion}/admin")]
    [Produces("application/json")]
    public class AgentsController : BaseController
    {
        public AgentsController(IAgentSystemSettingsManagementService systemSettingsManagementService)
        {
            _systemSettingsManagementService = systemSettingsManagementService;
        }
        
        
        [HttpPut("agencies/{agencyId}/agents/{agentId}/system-settings/availability-search")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> SetSystemSettings([FromBody] AvailabilitySearchSettings settings, [FromRoute] int agentId, [FromRoute] int agencyId)
        {
            var (_, isFailure, error) = await _systemSettingsManagementService.SetAvailabilitySearchSettings(agentId, agencyId, settings);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok();
        }
        
        
        private readonly IAgentSystemSettingsManagementService _systemSettingsManagementService;
    }
}