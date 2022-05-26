using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.ApiClients;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.ApiClients;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/api-client")]
    [Produces("application/json")]
    public class ApiClientsController : BaseController
    {
        public ApiClientsController(IApiClientService apiClientService, IAgentContextService agentContextService,
            IApiClientManagementService apiClientManagementService)
        {
            _apiClientService = apiClientService;
            _agentContextService = agentContextService;
            _apiClientManagementService = apiClientManagementService;
        }
        
        
        /// <summary>
        ///  Gets api client info for current agent
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiClientInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [InAgencyPermissions(InAgencyPermissions.ApiConnectionManagement)]
        public async Task<IActionResult> Get()
        {
            var agent = await _agentContextService.GetAgent();
            return OkOrBadRequest(await _apiClientService.GetCurrent(agent));
        }


        /// <summary>
        /// Generate a new api client for current agent
        /// </summary>
        /// <returns></returns>
        [HttpGet("generate")]
        [ProducesResponseType(typeof(ApiClientData), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [InAgencyPermissions(InAgencyPermissions.ApiConnectionManagement)]
        public async Task<IActionResult> Generate()
        {
            var agent = await _agentContextService.GetAgent();
            return OkOrBadRequest(await _apiClientManagementService.Generate(agent.AgencyId, agent.AgentId));
        }
        
        
        private readonly IApiClientService _apiClientService;
        private readonly IApiClientManagementService _apiClientManagementService;
        private readonly IAgentContextService _agentContextService;
    }
}