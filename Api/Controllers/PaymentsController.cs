using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/payments")]
    [Produces("application/json")]
    public class PaymentsController : BaseController
    {
        public PaymentsController(IPaymentService paymentService, ICreditCardService cardService)
        {
            _paymentService = paymentService;
            _cardService = cardService;
        }

        /// <summary>
        ///     Returns available currencies
        /// </summary>
        /// <returns>List of currencies.</returns>
        [HttpGet("currencies")]
        [ProducesResponseType(typeof(IReadOnlyCollection<Currencies>), (int) HttpStatusCode.OK)]
        public IActionResult GetCurrencies()
        {
            return Ok(_paymentService.GetCurrencies());
        }

        /// <summary>
        ///     Returns methods available for customer payments
        /// </summary>
        /// <returns>List of payment methods.</returns>
        [HttpGet("paymentMethods")]
        [ProducesResponseType(typeof(IReadOnlyCollection<PaymentMethods>),(int) HttpStatusCode.OK)]
        public IActionResult GetPaymentMethods()
        {
            return Ok(_paymentService.GetAvailableCustomerPaymentMethods());
        }

        /// <summary>
        ///     Returns available cards
        /// </summary>
        /// <returns>List of cards.</returns>
        [HttpGet("cards")]
        [ProducesResponseType(typeof(CreditCardInfo[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        private async Task<IActionResult> GetCards()
        {
            return OkOrBadRequest(await _cardService.GetAvailableCards());
        }

        /// <summary>
        ///     Make payment with new credit card
        /// </summary>
        /// <param name="request">Payment request with new credit card</param>
        [HttpPost("card/new")]
        [ProducesResponseType(typeof(PaymentResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        private async Task<IActionResult> PayWithNewCreditCard(PaymentWithNewCreditCardRequest request)
        {
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            return OkOrBadRequest(await _paymentService.PayWithNewCreditCard(request, LanguageCode, remoteIpAddress));
        }

        /// <summary>
        ///     Make payment with existing credit card
        /// </summary>
        /// <param name="request">Payment request with existing credit card</param>
        [HttpPost("card/existing")]
        [ProducesResponseType(typeof(PaymentResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        private async Task<IActionResult> PayWithExistingCreditCard(PaymentWithExistingCreditCardRequest  request)
        {
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            return OkOrBadRequest(await _paymentService.PayWithExistingCard(request, LanguageCode, remoteIpAddress));
        }

        private readonly IPaymentService _paymentService;
        private readonly ICreditCardService _cardService;
    }
}
