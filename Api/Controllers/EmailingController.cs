using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Emailing;
using HappyTravel.Edo.Api.Services.Mailing;
using Microsoft.AspNetCore.Mvc;


namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/emailing/")]
    [Produces("application/json")]
    public class EmailingController : ControllerBase
    {
        public EmailingController(IBookingMailingService bookingMailingService)
        {
            _bookingMailingService = bookingMailingService;
        }


        /// <summary>
        /// Sends booking voucher to an email.
        /// </summary>
        /// <param name="bookingId"></param>
        /// <param name="voucherRequest"></param>
        /// <returns></returns>
        [HttpPost("bookings/voucher/{bookingId}")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SendBookingVoucher([Required] int bookingId, [Required] [FromBody] SendVoucherRequest voucherRequest)
        {
            var (_, isFailure, error) = await _bookingMailingService.SendVoucher(bookingId, voucherRequest.Email);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok("Booking voucher has been sent");
        }


        /// <summary>
        /// Sends booking invoice to an email.
        /// </summary>
        /// <param name="bookingId"></param>
        /// <param name="voucherRequest"></param>
        /// <returns></returns>
        [HttpPost("bookings/invoice/{bookingId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SendBookingInvoice([Required] int bookingId, [Required] [FromBody] SendVoucherRequest voucherRequest)
        {
            var (_, isFailure, error) = await _bookingMailingService.SendInvoice(bookingId, voucherRequest.Email);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok("Booking invoice has been sent");
        }


        private readonly IBookingMailingService _bookingMailingService;
    }
}