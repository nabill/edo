﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.DirectApi.Models.Search;
using HappyTravel.Edo.DirectApi.Services.AvailabilitySearch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WideAvailabilitySearchService = HappyTravel.Edo.DirectApi.Services.AvailabilitySearch.WideAvailabilitySearchService;

namespace HappyTravel.Edo.DirectApi.Controllers
{
    /// <summary>
    /// These endpoints allow you to search accommodations for availability, get details, and prepare to make a booking.
    /// </summary>
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/{version:apiVersion}/availabilities/searches", Name = "Availability Search", Order = 2)]
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
        /// Wide availability: Start search
        /// </summary>
        /// <remarks>
        /// This endpoint starts the wide availability search for all available accommodations based on your search criteria.
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
        /// Wide availability: Get results
        /// </summary>
        /// <remarks>
        /// This endpoint returns the results of a wide availability search. It provides the available accommodations for a particular search ID. 
        /// If the search is still in progress, you receive a partial list and the `isComplete` flag is `false`.
        /// </remarks>
        [HttpGet("{searchId:guid}")]
        public async Task<ActionResult<WideAvailabilitySearchResult>> GetSearchResult(Guid searchId, CancellationToken cancellationToken)
        {
            var agent = await _agentContextService.GetAgent();
            var (isSuccess, _, result, error) = await _wideSearchService.GetResult(searchId, agent);

            return isSuccess
                ? result
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }
        
        /// <summary>
        /// Room selection
        /// </summary>
        /// <remarks>
        /// This endpoint narrows the results from a wide availability search. It accepts one search ID and one accommodation ID, 
        /// and it returns all matching room contract sets for the accommodation.
        /// </remarks>
        [HttpGet("{searchId:guid}/accommodations/{accommodationId}")]
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
        /// This endpoint confirms the final price and details for a room contract set and confirms that you can make a booking. 
        /// This temporarily ensures that no one else can book the room contract set while you make a decision and continue to the booking stage.
        /// </remarks>
        [HttpGet("{searchId:guid}/accommodations/{accommodationId}/room-contract-sets/{roomContractSetId:guid}")]
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