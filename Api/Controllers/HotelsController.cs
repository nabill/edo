using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Hotels;
using HappyTravel.Edo.Api.Services.Availabilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/hotels")]
    [Produces("application/json")]
    public class HotelsController : BaseController
    {
        public HotelsController(IHotelService service)
        {
            _service = service;
        }


        /// <summary>
        /// Returns the full list of hotel details. 
        /// </summary>
        /// <param name="hotelId"></param>
        /// <returns></returns>
        [HttpGet("{hotelId}")]
        [ProducesResponseType(typeof(RichHotelDetails), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async ValueTask<IActionResult> Get([FromRoute] string hotelId)
        {
            if (string.IsNullOrWhiteSpace(hotelId))
                return BadRequest(ProblemDetailsBuilder.Build($"No hotel IDs was provided."));

            var (_, isFailure, response, error) = await _service.Get(hotelId, LanguageCode);
            if (isFailure)
                return BadRequest(error);

            return Ok(response);
        }


        private readonly IHotelService _service;
    }
}
