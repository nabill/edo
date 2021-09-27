using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.CounterpartyStatesFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.NGenius;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Api.Services.Payments.CreditCards;
using HappyTravel.Edo.Api.Services.Payments.NGenius;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.MailSender.Infrastructure;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
            IAgentContextService agentContextService, ICreditCardPaymentProcessingService creditCardPaymentProcessingService, INGeniusPaymentService nGeniusPaymentService,
            NGeniusWebhookProcessingService nGeniusWebhookProcessingService)
        {
            _bookingPaymentCallbackService = bookingPaymentCallbackService;
            _paymentSettingsService = paymentSettingsService;
            _agentContextService = agentContextService;
            _creditCardPaymentProcessingService = creditCardPaymentProcessingService;
            _nGeniusPaymentService = nGeniusPaymentService;
            _nGeniusWebhookProcessingService = nGeniusWebhookProcessingService;
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
        [HttpGet("payment-processors")]
        [ProducesResponseType(typeof(IReadOnlyCollection<PaymentProcessors>), StatusCodes.Status200OK)]
        [AllowAnonymous]
        public IActionResult GetPaymentProcessors() 
            => Ok(_paymentSettingsService.GetPaymentProcessors());


        /// <summary>
        ///     Returns enabled payment system
        /// </summary>>
        [HttpGet("payment-processor")]
        [ProducesResponseType(typeof(PaymentProcessors), StatusCodes.Status200OK)]
        [AllowAnonymous]
        public IActionResult GetCurrentPaymentProcessor() 
            => Ok(_paymentSettingsService.GetCurrentPaymentProcessor());


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
        ///     Pays by NGenius
        /// </summary>
        /// <param name="referenceCode">Booking reference code</param>
        [HttpPost("accommodations/bookings/{referenceCode}/pay")]
        [ProducesResponseType(typeof(PaymentResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.FullAccess)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationBooking)]
        public async Task<IActionResult> PayByNGenius(string referenceCode)
        {
            return OkOrBadRequest(await _nGeniusPaymentService.Authorize(referenceCode, ClientIp, await _agentContextService.GetAgent()));
        }
        
        
        /// <summary>
        ///     Refreshes payment status in NGenius
        /// </summary>
        /// <param name="referenceCode">Booking reference code</param>
        [HttpPost("accommodations/bookings/{referenceCode}/pay/refresh-status")]
        [ProducesResponseType(typeof(PaymentStatuses), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RefreshStatusInNGenius(string referenceCode)
        {
            return OkOrBadRequest(await _nGeniusPaymentService.RefreshStatus(referenceCode));
        }


        /// <summary>
        ///     NGenius webhook
        /// </summary>
        [HttpPost("accommodations/bookings/ngenius/webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> NGeniusWebhook([FromBody] JsonDocument request)
        {
            await _nGeniusWebhookProcessingService.ProcessWebHook(request);
            return Ok();
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
        private readonly INGeniusPaymentService _nGeniusPaymentService;
        private readonly NGeniusWebhookProcessingService _nGeniusWebhookProcessingService;
    }
}