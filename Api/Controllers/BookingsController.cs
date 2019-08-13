using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Bookings;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/bookings")]
    [Produces("application/json")]
    public class BookingsController : BaseController
    {
        public BookingsController(IBookingService service)
        {
            _service = service;
        }


        /// <summary>
        ///     Book a hotel.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("hotels")]
        [ProducesResponseType(typeof(HotelBookingDetails), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> BookHotel([FromBody] HotelBookingRequest request)
        {
            var (_, isFailure, bookingDetails, error) = await _service.BookHotel(request, LanguageCode);
            if (isFailure)
                return BadRequest(error);

            return Ok(bookingDetails);
        }


        private readonly IBookingService _service;
    }
}