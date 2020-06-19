using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/payments")]
    [Produces("application/json")]
    public class PaymentsController : BaseController
    {
        public PaymentsController(IAccountPaymentService accountPaymentService,
            IBookingPaymentService bookingPaymentService, IPaymentSettingsService paymentSettingsService,
            IAgentContext agentContext, ICreditCardPaymentProcessingService creditCardPaymentProcessingService)
        {
            _accountPaymentService = accountPaymentService;
            _bookingPaymentService = bookingPaymentService;
            _paymentSettingsService = paymentSettingsService;
            _agentContext = agentContext;
            _creditCardPaymentProcessingService = creditCardPaymentProcessingService;
        }


        /// <summary>
        ///     Returns available currencies
        /// </summary>
        /// <returns>List of currencies.</returns>
        [HttpGet("currencies")]
        [ProducesResponseType(typeof(IReadOnlyCollection<Currencies>), (int) HttpStatusCode.OK)]
        public IActionResult GetCurrencies() => Ok(_paymentSettingsService.GetCurrencies());


        /// <summary>
        ///     Returns methods available for agent's payments
        /// </summary>
        /// <returns>List of payment methods.</returns>
        [HttpGet("methods")]
        [ProducesResponseType(typeof(IReadOnlyCollection<PaymentMethods>), (int) HttpStatusCode.OK)]
        public IActionResult GetPaymentMethods() => Ok(_paymentSettingsService.GetAvailableAgentPaymentMethods());


        /// <summary>
        ///     Pays by payfort token
        /// </summary>
        /// <param name="request">Payment request</param>
        [HttpPost("bookings/card/new")]
        [ProducesResponseType(typeof(PaymentResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AgentRequired]
        public async Task<IActionResult> PayWithNewCreditCard([FromBody] NewCreditCardPaymentRequest request)
        {
            return OkOrBadRequest(await _creditCardPaymentProcessingService.Authorize(request,
                LanguageCode,
                ClientIp,
                _bookingPaymentService));
        }


        /// <summary>
        ///     Pays by payfort token
        /// </summary>
        /// <param name="request">Payment request</param>
        [HttpPost("bookings/card/saved")]
        [ProducesResponseType(typeof(PaymentResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AgentRequired]
        public async Task<IActionResult> PayWithSavedCreditCard([FromBody] SavedCreditCardPaymentRequest request)
        {
            return OkOrBadRequest(await _creditCardPaymentProcessingService.Authorize(request,
                LanguageCode,
                ClientIp,
                _bookingPaymentService));
        }


        /// <summary>
        ///     Pays from account
        /// </summary>
        /// <param name="request">Payment request</param>
        [HttpPost("bookings/account")]
        [ProducesResponseType(typeof(PaymentResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AgentRequired]
        public async Task<IActionResult> PayWithAccount(AccountBookingPaymentRequest request)
        {
            var agent = await _agentContext.GetAgent();
            return OkOrBadRequest(await _accountPaymentService.AuthorizeMoney(request, agent, ClientIp));
        }


        /// <summary>
        ///     Processes payment callback
        /// </summary>
        [HttpPost("callback")]
        [ProducesResponseType(typeof(PaymentResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PaymentCallback([FromBody] JObject value)
            => OkOrBadRequest(await _creditCardPaymentProcessingService.ProcessPaymentResponse(value, _bookingPaymentService));


        /// <summary>
        ///     Returns account balance for currency
        /// </summary>
        /// <returns>Account balance</returns>
        [HttpGet("accounts/balance/{currency}")]
        [ProducesResponseType(typeof(AccountBalanceInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AgentRequired]
        public Task<IActionResult> GetAccountBalance(Currencies currency) => OkOrBadRequest(_accountPaymentService.GetAccountBalance(currency));


        private readonly IAgentContext _agentContext;
        private readonly ICreditCardPaymentProcessingService _creditCardPaymentProcessingService;
        private readonly IAccountPaymentService _accountPaymentService;
        private readonly IBookingPaymentService _bookingPaymentService;
        private readonly IPaymentSettingsService _paymentSettingsService;
    }
}