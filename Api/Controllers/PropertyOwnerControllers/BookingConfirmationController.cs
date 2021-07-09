using HappyTravel.Edo.Api.Models.PropertyOwners;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.PropertyOwners;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Controllers.PropertyOwnerControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/property-owner/confirmations")]
    [Produces("application/json")]
    public class BookingConfirmationController : BaseController
    {
        public BookingConfirmationController(IBookingConfirmationService bookingConfirmationService, IBookingInfoService bookingInfoService)
        {
            _bookingConfirmationService = bookingConfirmationService;
            _bookingInfoService = bookingInfoService;
        }


        /// <summary>
        ///     Gets an actual booking status and confirmation code
        /// </summary>
        /// <param name="referenceCode">Booking reference code</param>
        /// <returns>Booking status and property owner confirmation code</returns>
        [HttpGet("reference-code/{referenceCode}")]
        [ProducesResponseType(typeof(SlimBookingConfirmation), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get([FromRoute] string referenceCode)
        {
            return OkOrBadRequest(await _bookingConfirmationService.Get(referenceCode));
        }


        /// <summary>
        ///     Gets booking confirmation changes history
        /// </summary>
        /// <param name="referenceCode">Booking reference code for retrieving confirmation change history</param>
        /// <returns>List of booking confirmation change events</returns>
        [HttpGet("reference-code/{referenceCode}/confirmation-history")]
        [ProducesResponseType(typeof(List<BookingConfirmationHistoryEntry>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetBookingConfirmationCodeHistory([FromRoute] string referenceCode)
        {
            return OkOrBadRequest(await _bookingInfoService.GetBookingConfirmationHistory(referenceCode));
        }


        /// <summary>
        ///     Updates booking status and hotel confirmation code
        /// </summary>
        /// <param name="referenceCode">Booking reference code</param>
        /// <param name="bookingConfirmation">Booking confirmation data</param>
        /// <returns></returns>
        [HttpPut("reference-code/{referenceCode}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update([FromRoute] string referenceCode, [FromBody] BookingConfirmation bookingConfirmation)
            => NoContentOrBadRequest(await _bookingConfirmationService.Update(referenceCode, bookingConfirmation));


        private readonly IBookingConfirmationService _bookingConfirmationService;
        private readonly IBookingInfoService _bookingInfoService;
    }
}
