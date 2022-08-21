using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Models.ApiClients;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    /// <summary>
    /// API for managing direct api clients
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/direct-api-clients")]
    [Produces("application/json")]
    public class DirectApiClientsController : BaseController
    {
        public DirectApiClientsController(IDirectApiClientManagementService directApiClientManagementService, IAgentContextService agentContextService)
        {
            _directApiClientManagementService = directApiClientManagementService;
            _agentContextService = agentContextService;
        }
        
        
        /// <summary>
        /// Creates new direct api client in identity service
        /// </summary>
        /// <returns></returns>
        [HttpGet("generate")]
        [ProducesResponseType(typeof(ApiClientInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [InAgencyPermissions(InAgencyPermissions.ApiConnectionManagement)]
        public async Task<IActionResult> AddApiClient()
        {
            var agent = await _agentContextService.GetAgent();
            return NoContentOrBadRequest(await _directApiClientManagementService.Generate(agent));
        }


        private readonly IDirectApiClientManagementService _directApiClientManagementService;
        private readonly IAgentContextService _agentContextService;
    }
}