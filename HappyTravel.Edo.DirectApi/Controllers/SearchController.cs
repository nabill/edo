using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.DirectApi.Models;
using HappyTravel.Edo.DirectApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WideAvailabilitySearchService = HappyTravel.Edo.DirectApi.Services.WideAvailabilitySearchService;

namespace HappyTravel.Edo.DirectApi.Controllers
{
    /// <summary>
    /// <h2>The booking flow contains four following steps:</h2>
    /// <ul>
    /// <li>Wide availability search for search all available accommodations on predefined parameters.</li>
    /// <li>Room selection for getting a specific contract from a selected accommodation.</li>
    /// <li>Booking evaluation to ensure no one book a contract you want when you make a decision and fill out passenger data.</li>
    /// <li>Booking to book the selected contract.</li>
    /// </ul>
    /// </summary>
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/{version:apiVersion}/availabilities/searches")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class SearchController : ControllerBase
    {
        public SearchController(IAgentContextService agentContextService, WideAvailabilitySearchService wideSearchService,
            AccommodationAvailabilitiesService accommodationAvailabilitiesService, ValuationService valuationService)
        {
            _agentContextService = agentContextService;
            _wideSearchService = wideSearchService;
            _accommodationAvailabilitiesService = accommodationAvailabilitiesService;
            _valuationService = valuationService;
        }


        /// <summary>
        /// Starting search
        /// </summary>
        /// <remarks>
        /// Starting wide availability search for search all available accommodations on predefined parameters.
        /// </remarks>
        [HttpPost]
        public async Task<ActionResult<StartSearchResponse>> StartSearch([FromBody] AvailabilityRequest request, CancellationToken cancellationToken)
        {
            var agent = await _agentContextService.GetAgent();
            var (isSuccess, _, searchResponse, error) = await _wideSearchService.StartSearch(request, agent, "en");

            return isSuccess
                ? searchResponse
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }

        /// <summary>
        /// Getting accommodations
        /// </summary>
        /// <remarks>
        /// Returns all available accommodations for provided searchId
        /// </remarks>
        [HttpGet("{searchId}")]
        public async Task<ActionResult<WideSearchResult>> GetSearchResult(Guid searchId, CancellationToken cancellationToken)
        {
            var agent = await _agentContextService.GetAgent();
            var (isSuccess, _, result, error) = await _wideSearchService.GetResult(searchId, agent, "en");

            return isSuccess
                ? result
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }
        
        /// <summary>
        /// Room selection
        /// </summary>
        /// <remarks>
        /// Returns room contract sets for getting a specific contract from a selected accommodation.
        /// </remarks>
        [HttpGet("{searchId}/accommodations/{accommodationId}")]
        public async Task<ActionResult<RoomSelectionResult>> GetAvailabilityForAccommodation(Guid searchId, string accommodationId, CancellationToken cancellationToken)
        {
            var agent = await _agentContextService.GetAgent();
            var (isSuccess, _, result, error) = await _accommodationAvailabilitiesService.Get(searchId, accommodationId, agent, "en");
            
            return isSuccess
                ? result
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }
        
        /// <summary>
        /// Booking evaluation
        /// </summary>
        /// <remarks>
        /// Booking evaluation to ensure no one book a contract you want when you make a decision and fill out passenger data.
        /// </remarks>
        [HttpGet("{searchId}/accommodations/{accommodationId}/room-contract-sets/{roomContractSetId}")]
        public async Task<ActionResult<RoomContractSetAvailability>> GetExactAvailability(Guid searchId, string accommodationId, Guid roomContractSetId, CancellationToken cancellationToken)
        {
            var agent = await _agentContextService.GetAgent();
            var (isSuccess, _, result, error) = await _valuationService.Get(searchId, accommodationId, roomContractSetId, agent, "en");
            
            return isSuccess
                ? result
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        private readonly IAgentContextService _agentContextService;
        private readonly WideAvailabilitySearchService _wideSearchService;
        private readonly AccommodationAvailabilitiesService _accommodationAvailabilitiesService;
        private readonly ValuationService _valuationService;
    }
}