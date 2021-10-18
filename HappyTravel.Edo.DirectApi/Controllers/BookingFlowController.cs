using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.DirectApi.Models;
using HappyTravel.Edo.DirectApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    [Route("api/{version:apiVersion}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class BookingFlowController : ControllerBase
    {
        public BookingFlowController(IAgentContextService agentContextService, WideSearchService wideSearchService, 
            AccommodationAvailabilitiesService accommodationAvailabilitiesService)
        {
            _agentContextService = agentContextService;
            _wideSearchService = wideSearchService;
            _accommodationAvailabilitiesService = accommodationAvailabilitiesService;
        }


        /// <summary>
        /// Starting search
        /// </summary>
        /// <remarks>
        /// Starting wide availability search for search all available accommodations on predefined parameters.
        /// </remarks>
        [HttpPost("searches")]
        public async Task<ActionResult<StartSearchResponse>> StartSearch([FromBody] AvailabilityRequest request, CancellationToken cancellationToken)
        {
            var agent = await _agentContextService.GetAgent();
            var (isSuccess, _, searchResponse, error) = await _wideSearchService.StartSearch(request, agent, "en");

            return isSuccess
                ? searchResponse
                : BadRequest(error);
        }

        /// <summary>
        /// Getting accommodations
        /// </summary>
        /// <remarks>
        /// Returns all available accommodations for provided searchId
        /// </remarks>
        [HttpGet("searches/{searchId}")]
        public async Task<ActionResult<WideSearchResult>> GetSearchResult(Guid searchId, CancellationToken cancellationToken)
        {
            var agent = await _agentContextService.GetAgent();
            var (isSuccess, _, result, error) = await _wideSearchService.GetResult(searchId, agent);

            return isSuccess
                ? result
                : BadRequest(error);
        }
        
        /// <summary>
        /// Room selection
        /// </summary>
        /// <remarks>
        /// Returns room contract sets for getting a specific contract from a selected accommodation.
        /// </remarks>
        [HttpGet("searches/{searchId}/results/{htId}")]
        public async Task<ActionResult<RoomSelectionResult>> GetAvailabilityForAccommodation(Guid searchId, string htId, CancellationToken cancellationToken)
        {
            var agent = await _agentContextService.GetAgent();
            var (isSuccess, _, result, error) = await _accommodationAvailabilitiesService.Get(searchId, htId, agent);
            
            return isSuccess
                ? result
                : BadRequest(error);
        }
        
        /// <summary>
        /// Booking evaluation
        /// </summary>
        /// <remarks>
        /// Booking evaluation to ensure no one book a contract you want when you make a decision and fill out passenger data.
        /// </remarks>
        [HttpGet("searches/{searchId}/results/{htId}/room-contract-sets/{roomContractSetId}")]
        public async Task<ActionResult<RoomContractSetAvailability>> GetExactAvailability(Guid searchId, string htId, Guid roomContractSetId, CancellationToken cancellationToken)
        {
            return Ok();
        }
        
        /// <summary>
        /// Creating booking.
        /// </summary>
        /// <remarks>
        /// Booking selected contract
        /// </remarks>
        [HttpPost("book")]
        public async Task<IActionResult> Book([FromBody] AccommodationBookingRequest request)
        {
            return Ok();
        }
        
        
        private readonly IAgentContextService _agentContextService;
        private readonly WideSearchService _wideSearchService;
        private readonly AccommodationAvailabilitiesService _accommodationAvailabilitiesService;
    }
}