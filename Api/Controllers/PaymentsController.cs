using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Customers;
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
        public PaymentsController(IPaymentService paymentService, ITokenizationService tokenizationService, ICustomerContext customerContext)
        {
            _paymentService = paymentService;
            _tokenizationService = tokenizationService;
            _customerContext = customerContext;
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
        ///     Get one time payment token
        /// </summary>
        /// <param name="request">Get one time payment token request</param>
        [HttpPost("token/card/one_time")]
        [ProducesResponseType(typeof(GetTokenResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetOneTimeToken(GetOneTimeTokenRequest request)
        {
            var (_, customerFailure, customer, customerError) = await _customerContext.GetCustomer();
            if (customerFailure)
                return BadRequest(ProblemDetailsBuilder.Build(customerError));
            
            return OkOrBadRequest(await _tokenizationService.GetOneTimeToken(request, LanguageCode, customer));
        }

        /// <summary>
        ///     Get payment token for saved card
        /// </summary>
        /// <param name="request">Get payment token for saved card request</param>
        [HttpPost("token/card/existing")]
        [ProducesResponseType(typeof(GetTokenResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetTokenForExistingCard(GetTokenRequest request)
        {
            var (_, companyFailure, company, companyError) = await _customerContext.GetCompany();
            if (companyFailure)
                return BadRequest(ProblemDetailsBuilder.Build(companyError));

            var (_, customerFailure, customer, customerError) = await _customerContext.GetCustomer();
            if (customerFailure)
                return BadRequest(ProblemDetailsBuilder.Build(customerError));
            
            return OkOrBadRequest(await _tokenizationService.GetToken(request, customer, company));
        }
        /// <summary>
        ///     Pay by token
        /// </summary>
        /// <param name="request">Payment request</param>
        [HttpPost()]
        [ProducesResponseType(typeof(PaymentResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PayByToken(PaymentRequest request)
        {
            var (_, companyFailure, company, companyError) = await _customerContext.GetCompany();
            if (companyFailure)
                return BadRequest(ProblemDetailsBuilder.Build(companyError));

            var (_, customerFailure, customer, customerError) = await _customerContext.GetCustomer();
            if (customerFailure)
                return BadRequest(ProblemDetailsBuilder.Build(customerError));
            throw new NotImplementedException();
            //return OkOrBadRequest(await _tokenizationService.GetToken(request, customer, company));
        }

        private readonly IPaymentService _paymentService;
        private readonly ITokenizationService _tokenizationService;
        private readonly ICustomerContext _customerContext;
    }
}
