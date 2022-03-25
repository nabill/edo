using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Models.ApiClients;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.ApiClients;
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
        public ApiClientsController(IApiClientService apiClientService,
            IAgentContextService agentContextService)
        {
            _apiClientService = apiClientService;
            _agentContextService = agentContextService;
        }
        
        
        /// <summary>
        ///  Gets api client info for current agent
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiClientInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationBooking)]
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
        [ProducesResponseType(typeof(GeneratedApiClient), (int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationBooking)]
        public async Task<IActionResult> Generate()
        {
            var agent = await _agentContextService.GetAgent();
            return Ok(await _apiClientService.GenerateApiClient(agent));
            
        }
        
        
        private readonly IApiClientService _apiClientService;
        private readonly IAgentContextService _agentContextService;
    }
}