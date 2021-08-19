using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.CounterpartyStatesFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/payments")]
    [Produces("application/json")]
    public class PaymentsController : BaseController
    {
        public PaymentsController(IBookingPaymentCallbackService bookingPaymentCallbackService, IPaymentSettingsService paymentSettingsService,
            IAgentContextService agentContextService, ICreditCardPaymentProcessingService creditCardPaymentProcessingService)
        {
            _bookingPaymentCallbackService = bookingPaymentCallbackService;
            _paymentSettingsService = paymentSettingsService;
            _agentContextService = agentContextService;
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
        ///     Returns available payment systems
        /// </summary>>
        [HttpGet("payment-systems")]
        [ProducesResponseType(typeof(IReadOnlyCollection<PaymentSystems>), StatusCodes.Status200OK)]
        public IActionResult GetPaymentSystems() 
            => Ok(_paymentSettingsService.GetPaymentSystems());


        /// <summary>
        ///     Returns enabled payment system
        /// </summary>>
        [HttpGet("payment-system")]
        [ProducesResponseType(typeof(PaymentSystems), StatusCodes.Status200OK)]
        public IActionResult GetEnabledPaymentSystem() 
            => Ok(_paymentSettingsService.GetEnabledPaymentSystem());


        /// <summary>
        ///     Returns methods available for agent's payments
        /// </summary>
        /// <returns>List of payment methods.</returns>
        [HttpGet("methods")]
        [ProducesResponseType(typeof(IReadOnlyCollection<PaymentTypes>), (int) HttpStatusCode.OK)]
        public IActionResult GetPaymentTypes() 
            => Ok(_paymentSettingsService.GetAvailableAgentPaymentTypes());


        /// <summary>
        ///     Pays by payfort token
        /// </summary>
        /// <param name="request">Payment request</param>
        [HttpPost("accommodations/bookings/cards/new/pay")]
        [ProducesResponseType(typeof(PaymentResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.FullAccess)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationBooking)]
        public async Task<IActionResult> PayWithNewCreditCard([FromBody] NewCreditCardPaymentRequest request)
        {
            return OkOrBadRequest(await _creditCardPaymentProcessingService.Authorize(request,
                LanguageCode,
                ClientIp,
                _bookingPaymentCallbackService,
                await _agentContextService.GetAgent()));
        }


        /// <summary>
        ///     Pays by payfort token
        /// </summary>
        /// <param name="request">Payment request</param>
        [HttpPost("accommodations/bookings/cards/saved/pay")]
        [ProducesResponseType(typeof(PaymentResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.FullAccess)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationBooking)]
        public async Task<IActionResult> PayWithSavedCreditCard([FromBody] SavedCreditCardPaymentRequest request)
        {
            return OkOrBadRequest(await _creditCardPaymentProcessingService.Authorize(request,
                LanguageCode,
                ClientIp,
                _bookingPaymentCallbackService,
                await _agentContextService.GetAgent()));
        }


        /// <summary>
        ///     Processes payment callback
        /// </summary>
        [HttpPost("callback")]
        [ProducesResponseType(typeof(PaymentResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.FullAccess)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationBooking)]
        public async Task<IActionResult> PaymentCallback([FromBody] JObject value)
            => OkOrBadRequest(await _creditCardPaymentProcessingService.ProcessPaymentResponse(value, _bookingPaymentCallbackService));


        private readonly IAgentContextService _agentContextService;
        private readonly ICreditCardPaymentProcessingService _creditCardPaymentProcessingService;
        private readonly IBookingPaymentCallbackService _bookingPaymentCallbackService;
        private readonly IPaymentSettingsService _paymentSettingsService;
    }
}