using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Mappings;
using HappyTravel.Edo.Api.Services.Agents;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/accommodations-mapping")]
    [Produces("application/json")]
    public class AccommodationMappingController : BaseController
    {
        public AccommodationMappingController(AccommodationDuplicatesService duplicatesService, IAgentContextService agentContextService)
        {
            _duplicatesService = duplicatesService;
            _agentContextService = agentContextService;
        }


        /// <summary>
        /// Adds an accommodation duplicate report.
        /// </summary>
        /// <param name="request">Duplicate accommodation info</param>
        /// <returns></returns>
        [HttpPost("duplicate-reports")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [AgentRequired]
        public async Task<IActionResult> AddDuplicateReport([FromBody] ReportAccommodationDuplicateRequest request)
        {
            var agent = await _agentContextService.GetAgent();
            return OkOrBadRequest(await _duplicatesService.Report(request, agent));
        }
        
        private readonly AccommodationDuplicatesService _duplicatesService;
        private readonly IAgentContextService _agentContextService;
    }
}