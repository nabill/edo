using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Locations;
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
        public LocationsController(ILocationService service)
        {
            _service = service;
        }


        /// <summary>
        ///     Returns a list of world countries.
        /// </summary>
        /// <param name="query">The search query text.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("countries")]
        [ProducesResponseType(typeof(List<Country>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCountries([FromQuery] string query)
            => Ok(await _service.GetCountries(query, LanguageCode));


        /// <summary>
        ///     Returns a list of markets.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("markets")]
        [ProducesResponseType(typeof(List<Market>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMarkets()
            => Ok(await _service.GetMarkets(LanguageCode));


        private readonly ILocationService _service;
    }
}