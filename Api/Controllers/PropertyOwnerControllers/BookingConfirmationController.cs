using HappyTravel.Edo.Api.Models.PropertyOwners;
using HappyTravel.Edo.Api.Services.PropertyOwners;
using Microsoft.AspNetCore.Mvc;
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
        public BookingConfirmationController(IBookingConfirmationService bookingConfirmationService)
        {
            _bookingConfirmationService = bookingConfirmationService;
        }


        /// <summary>
        ///     Gets an actual booking status and confirmation code
        /// </summary>
        /// <param name="referenceCode">Booking reference code</param>
        /// <returns>Agency availability search settings</returns>
        [HttpGet("{referenceCode}")]
        [ProducesResponseType(typeof(AgencyAccommodationBookingSettingsInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> Get([FromRoute] string referenceCode)
        {
            var (_, isFailure, settings, error) = await _bookingConfirmationService.Get(referenceCode);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            if (settings == default)
                return NoContent();

            return Ok(settings.ToAgencyAccommodationBookingSettingsInfo());
        }


        /// <summary>
        ///     Updates booking status and hotel confirmation code
        /// </summary>
        /// <param name="referenceCode">Booking reference code</param>
        /// <param name="bookingConfirmation">Booking confirmation data</param>
        /// <returns></returns>
        [HttpPut("{referenceCode}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Update([FromRoute] string referenceCode, [FromBody] BookingConfirmation bookingConfirmation)
            => NoContentOrBadRequest(await _bookingConfirmationService.Update(referenceCode, bookingConfirmation));


        private readonly IBookingConfirmationService _bookingConfirmationService;
    }
}
