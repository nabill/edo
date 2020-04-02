using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Filters.Authorization.CustomerExistingFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.EdoContracts.General;
using HappyTravel.EdoContracts.General.Enums;
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
            IBookingPaymentService bookingPaymentService, IPaymentService paymentService,
            ICustomerContext customerContext, ICreditCardPaymentProcessingService creditCardPaymentProcessingService)
        {
            _accountPaymentService = accountPaymentService;
            _bookingPaymentService = bookingPaymentService;
            _paymentService = paymentService;
            _customerContext = customerContext;
            _creditCardPaymentProcessingService = creditCardPaymentProcessingService;
        }


        /// <summary>
        ///     Returns available currencies
        /// </summary>
        /// <returns>List of currencies.</returns>
        [HttpGet("currencies")]
        [ProducesResponseType(typeof(IReadOnlyCollection<Currencies>), (int) HttpStatusCode.OK)]
        public IActionResult GetCurrencies() => Ok(_paymentService.GetCurrencies());


        /// <summary>
        ///     Returns methods available for customer's payments
        /// </summary>
        /// <returns>List of payment methods.</returns>
        [HttpGet("methods")]
        [ProducesResponseType(typeof(IReadOnlyCollection<PaymentMethods>), (int) HttpStatusCode.OK)]
        public IActionResult GetPaymentMethods() => Ok(_paymentService.GetAvailableCustomerPaymentMethods());


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
        [CustomerRequired]
        public async Task<IActionResult> PayWithNewCreditCard([FromBody] NewCreditCardPaymentRequest request)
        {
            return OkOrBadRequest(await _creditCardPaymentProcessingService.AuthorizeMoney(request,
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
        [CustomerRequired]
        public async Task<IActionResult> PayWithSavedCreditCard([FromBody] SavedCreditCardPaymentRequest request)
        {
            return OkOrBadRequest(await _creditCardPaymentProcessingService.AuthorizeMoney(request,
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
        [CustomerRequired]
        public async Task<IActionResult> PayWithAccount(AccountBookingPaymentRequest request)
        {
            var customer = await _customerContext.GetCustomer();
            return OkOrBadRequest(await _accountPaymentService.AuthorizeMoney(request, customer, ClientIp));
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
        [CustomerRequired]
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


        private readonly ICustomerContext _customerContext;
        private readonly ICreditCardPaymentProcessingService _creditCardPaymentProcessingService;
        private readonly IAccountPaymentService _accountPaymentService;
        private readonly IBookingPaymentService _bookingPaymentService;
        private readonly IPaymentService _paymentService;
    }
}