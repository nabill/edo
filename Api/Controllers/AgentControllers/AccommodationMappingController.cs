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
    [Route("api/{v:apiVersion}/accommodation-mapping")]
    [Produces("application/json")]
    public class AccommodationMappingController : BaseController
    {
        public AccommodationMappingController(AccommodationDuplicatesReportService duplicatesReportService, IAgentContextService agentContextService)
        {
            _duplicatesReportService = duplicatesReportService;
            _agentContextService = agentContextService;
        }


        /// <summary>
        /// Adds an accommodation duplicate report.
        /// </summary>
        /// <param name="request">Duplicate accommodation info</param>
        /// <returns></returns>
        [HttpPost("duplicate-reports")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [AgentRequired]
        public async ValueTask<IActionResult> AddDuplicateReport([FromBody] ReportAccommodationDuplicateRequest request)
        {
            var agent = await _agentContextService.GetAgent();
            await _duplicatesReportService.Add(request, agent);
            
            return NoContent();
        }
        
        private readonly AccommodationDuplicatesReportService _duplicatesReportService;
        private readonly IAgentContextService _agentContextService;
    }
}