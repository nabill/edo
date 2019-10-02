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
        public PaymentsController(IPaymentService paymentService, ICustomerContext customerContext)
        {
            _paymentService = paymentService;
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
        ///     Appends money to specified account.
        /// </summary>
        /// <param name="accountId">Id of account to add money.</param>
        /// <param name="paymentData">Payment details.</param>
        /// <returns></returns>
        [HttpPost("{accountId}/replenish")]
        [ProducesResponseType(typeof(IReadOnlyCollection<PaymentMethods>),(int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> ReplenishAccount(int accountId, [FromBody] PaymentData paymentData)
        {
            var (isSuccess, _, error) = await _paymentService.ReplenishAccount(accountId, paymentData);
            return isSuccess 
                ? (IActionResult) NoContent()
                : (IActionResult) BadRequest(ProblemDetailsBuilder.Build(error));
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

            return OkOrBadRequest(await _paymentService.Pay(request, LanguageCode, GetIp(), customer, company));
        }

        private string GetIp()
        {            
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            return remoteIpAddress;
        }

        private readonly IPaymentService _paymentService;
        private readonly ICustomerContext _customerContext;
    }
}
