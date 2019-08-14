using System;
using System.Collections.Generic;
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
        ///     Book an accommodation.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("accommodations")]
        [ProducesResponseType(typeof(AccommodationBookingDetails), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> BookAccommodation([FromBody] AccommodationBookingRequest request)
        {
            return Ok(new AccommodationBookingDetails("some-code", BookingStatusCodes.Confirmed, request.CheckInDate, request.CheckOutDate, "XX", request.AccommodationId,
                request.TariffCode, 0, DateTime.MaxValue, new List<BookingRoomDetailsWithPrice>()));

            /*var (_, isFailure, bookingDetails, error) = await _service.BookAccommodation(request, LanguageCode);
            if (isFailure)
                return BadRequest(error);

            return Ok(bookingDetails);*/
        }


        private readonly IBookingService _service;
    }
}