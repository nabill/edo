using HappyTravel.Edo.Api.Models.Hotels;
using HappyTravel.Edo.Api.Services.PropertyOwners;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Controllers.HotelControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/hotel/confirmation")]
    [Produces("application/json")]
    public class BookingConfirmationController : BaseController
    {
        public BookingConfirmationController(IBookingConfirmationService hotelConfirmationService)
        {
            _hotelConfirmationService = hotelConfirmationService;
        }


        /// <summary>
        ///     Updates booking status and hotel confirmation code
        /// </summary>
        /// <param name="hotelConfirmation">Settings</param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update([FromBody] BookingConfirmation hotelConfirmation)
            => NoContentOrBadRequest(await _hotelConfirmationService.Update(hotelConfirmation));


        private readonly IBookingConfirmationService _hotelConfirmationService;
    }
}
