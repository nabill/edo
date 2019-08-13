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
    public class AvailabilitiesController : BaseController
    {
        public AvailabilitiesController(IAvailabilityService service)
        {
            _service = service;
        }


        /// <summary>
        /// Returns accommodations available for a booking.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("accommodations")]
        [ProducesResponseType(typeof(AvailabilityResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAccommodations([FromBody] AvailabilityRequest request)
        {
            var (_, isFailure, response, error) = await _service.Get(request, LanguageCode);
            if (isFailure)
                return BadRequest(error);

            return Ok(response);
        }


        private readonly IAvailabilityService _service;
    }
}
