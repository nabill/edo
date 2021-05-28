using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Filters.Authorization.ServiceAccountFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Locations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/locations")]
    [Produces("application/json")]
    public class LocationsController : BaseController
    {
        public LocationsController(IAgentContextService agentContextService, ILocationService service, ILocationNormalizer locationNormalizer)
        {
            _agentContextService = agentContextService;
            _service = service;
            _locationNormalizer = locationNormalizer;
        }


        /// <summary>
        ///     Returns a list of world countries.
        /// </summary>
        /// <param name="query">The search query text.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("countries")]
        [ProducesResponseType(typeof(List<Country>), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetCountries([FromQuery] string query) 
            => Ok(await _service.GetCountries(query, LanguageCode));


        /// <summary>
        ///     Returns a list of world regions.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("regions")]
        [ProducesResponseType(typeof(List<Region>), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetRegions() => Ok(await _service.GetRegions(LanguageCode));


        /// <summary>
        /// Starts a locations normalization process
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [HttpPost("normalize")]
        [ServiceAccountRequired]
        public async Task<IActionResult> Normalize()
        {
            await _locationNormalizer.StartNormalization();
            return NoContent();
        }


        private readonly IAgentContextService _agentContextService;
        private readonly ILocationService _service;
        private readonly ILocationNormalizer _locationNormalizer;
    }
}