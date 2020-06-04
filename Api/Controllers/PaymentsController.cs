using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management.Enums;
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
        ///     Appends money to specified account.
        /// </summary>
        /// <param name="accountId">Id of account to add money.</param>
        /// <param name="paymentData">Payment details.</param>
        /// <returns></returns>
        [HttpPost("{accountId}/replenish")]
        [ProducesResponseType(typeof(IReadOnlyCollection<PaymentMethods>), (int) HttpStatusCode.NoContent)]
        [AdministratorPermissions(AdministratorPermissions.AccountReplenish)]
        public async Task<IActionResult> ReplenishAccount(int accountId, [FromBody] PaymentData paymentData)
        {
            var (isSuccess, _, error) = await _accountPaymentService.ReplenishAccount(accountId, paymentData);
            return isSuccess
                ? NoContent()
                : (IActionResult) BadRequest(ProblemDetailsBuilder.Build(error));
        }


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


        /// <summary>
        ///     Completes payment manually
        /// </summary>
        /// <param name="bookingId">Booking id for completion</param>
        [HttpPost("offline/{bookingId}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CompleteOffline(int bookingId)
        {
            var (_, isFailure, error) = await _bookingPaymentService.CompleteOffline(bookingId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Gets balance for a counterparty account
        /// </summary>
        /// <param name="counterpartyId"></param>
        /// <param name="currency"></param>
        [HttpGet("counterparties/{counterpartyId}/balance/{currency}")]
        [ProducesResponseType(typeof(CounterpartyBalanceInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyBalanceObservation)]
        public async Task<IActionResult> GetCounterpartyBalance(int counterpartyId, Currencies currency) =>
            OkOrBadRequest(await _accountPaymentService.GetCounterpartyBalance(counterpartyId, currency));


        /// <summary>
        ///     Appends money to a counterparty account
        /// </summary>
        /// <param name="counterpartyAccountId"></param>
        /// <param name="paymentData"></param>
        [HttpPost("counterparty-accounts/{counterpartyAccountId}/replenish")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ReplenishCounterpartyAccount(int counterpartyAccountId, [FromBody] PaymentData paymentData)
        {
            var (isSuccess, _, error) = await _accountPaymentService.ReplenishCounterpartyAccount(counterpartyAccountId, paymentData);
            return isSuccess
                ? NoContent()
                : (IActionResult)BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        ///     Subtracts money from a counterparty account due to payment cancellation
        /// </summary>
        /// <param name="counterpartyAccountId"></param>
        /// <param name="cancellationData"></param>
        [HttpPost("counterparty-accounts/{counterpartyAccountId}/subtract")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SubtractCounterpartyAccount(int counterpartyAccountId, [FromBody] PaymentCancellationData cancellationData)
        {
            var (isSuccess, _, error) = await _accountPaymentService.SubtractMoneyCounterparty(counterpartyAccountId, cancellationData);
            return isSuccess
                ? NoContent()
                : (IActionResult)BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        ///     Transfers money from a counterparty account to the default agency account
        /// </summary>
        /// <param name="counterpartyAccountId"></param>
        /// <param name="transferData"></param>
        [HttpPost("counterparty-accounts/{counterpartyAccountId}/transfer")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> TransferToDefaultAgency(int counterpartyAccountId, [FromBody] TransferData transferData)
        {
            var (isSuccess, _, error) = await _accountPaymentService.TransferToDefaultAgency(counterpartyAccountId, transferData);
            return isSuccess
                ? NoContent()
                : (IActionResult)BadRequest(ProblemDetailsBuilder.Build(error));
        }


        private readonly IAgentContext _agentContext;
        private readonly ICreditCardPaymentProcessingService _creditCardPaymentProcessingService;
        private readonly IAccountPaymentService _accountPaymentService;
        private readonly IBookingPaymentService _bookingPaymentService;
        private readonly IPaymentSettingsService _paymentSettingsService;
    }
}