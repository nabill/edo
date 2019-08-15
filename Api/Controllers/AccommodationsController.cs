using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Availabilities;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/accommodations")]
    [Produces("application/json")]
    public class AccommodationsController : BaseController
    {
        public AccommodationsController(IAccommodationService service)
        {
            _service = service;
        }


        /// <summary>
        ///     Returns the full set of accommodation details.
        /// </summary>
        /// <param name="accommodationId">Accommodation ID, obtained from an availability query.</param>
        /// <returns></returns>
        [HttpGet("{accommodationId}")]
        [ProducesResponseType(typeof(RichAccommodationDetails), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async ValueTask<IActionResult> Get([FromRoute] string accommodationId)
        {
            if (string.IsNullOrWhiteSpace(accommodationId))
                return BadRequest(ProblemDetailsBuilder.Build("No accommodation IDs was provided."));

            var (_, isFailure, response, error) = await _service.Get(accommodationId, LanguageCode);
            if (isFailure)
                return BadRequest(error);

            return Ok(response);
        }


        private readonly IAccommodationService _service;
    }
}