using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
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
        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
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
        [ProducesResponseType(typeof(IReadOnlyCollection<CardInfo>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        private async Task<IActionResult> GetCards()
        {
            var (_, isFailure, value, error) = await _paymentService.GetAvailableCards();
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(value);
        }

        /// <summary>
        ///     Make payment with new card
        /// </summary>
        /// <param name="request">New card request</param>
        [HttpPost]
        [ProducesResponseType(typeof(PaymentResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        private async Task<IActionResult> PayNewCard(NewCardPaymentRequest request)
        {
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var (_, isFailure, response, error) = await _paymentService.NewCardPay(request, LanguageCode, remoteIpAddress);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(response);
        }

        /// <summary>
        ///     Make payment with saved card
        /// </summary>
        /// <param name="request">Saved card request</param>
        [HttpPut]
        [ProducesResponseType(typeof(PaymentResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        private async Task<IActionResult> PaySavedCard(SavedCardPaymentRequest request)
        {
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var (_, isFailure, response, error) = await _paymentService.SavedCardPay(request, LanguageCode, remoteIpAddress);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(response);
        }

        private readonly IPaymentService _paymentService;
    }
}