using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments.External;
using HappyTravel.Edo.Api.Services.PaymentLinks;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/external/payment-links")]
    [Produces("application/json")]
    public class PaymentLinksController : BaseController
    {
        public PaymentLinksController(IPaymentLinkService paymentLinkService)
        {
            _paymentLinkService = paymentLinkService;
        }


        /// <summary>
        ///     Gets supported desktop client versions.
        /// </summary>
        /// <returns>List of supported versions.</returns>
        [HttpGet("versions")]
        [ProducesResponseType(typeof(List<Version>), (int) HttpStatusCode.OK)]
        public IActionResult GetSupportedDesktopAppVersion() => Ok(_paymentLinkService.GetSupportedVersions());


        /// <summary>
        ///     Gets settings for payment links.
        /// </summary>
        /// <returns>Payment link settings.</returns>
        [HttpGet("settings")]
        [ProducesResponseType(typeof(PaymentLinkSettings), (int) HttpStatusCode.OK)]
        public IActionResult GetSettings() => Ok(_paymentLinkService.GetSettings());


        /// <summary>
        ///     Sends payment link to specified e-mail address.
        /// </summary>
        /// <param name="request">Send link request</param>
        /// <returns></returns>
        [HttpPost("send")]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> SendLink([FromBody] SendPaymentLinkRequest request)
        {
            var (isSuccess, _, error) = await _paymentLinkService.Send(request.Email, request.PaymentData);
            return isSuccess
                ? NoContent()
                : (IActionResult) BadRequest(ProblemDetailsBuilder.Build(error));
        }


        private readonly IPaymentLinkService _paymentLinkService;
    }
}