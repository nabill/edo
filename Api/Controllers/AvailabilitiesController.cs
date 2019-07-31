using System.Net;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Services.Availabilities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/availabilities")]
    [Produces("application/json")]
    public class AvailabilitiesController : ControllerBase
    {
        public AvailabilitiesController(IAvailabilityService service)
        {
            _service = service;
        }


        /// <summary>
        /// Returns hotels available for a booking.
        /// </summary>
        /// <param name="languageCode"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(AvailabilityResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get([FromQuery] string languageCode, [FromBody] AvailabilityRequest request)
        {
            var (_, isFailure, response, error) = await _service.Get(request, languageCode);
            if (isFailure)
                return BadRequest(error);

            return Ok(response);
        }


        private readonly IAvailabilityService _service;
    }
}
