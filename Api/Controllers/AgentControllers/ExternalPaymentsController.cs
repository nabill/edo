using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.NGenius;
using HappyTravel.Edo.Api.Services.Payments.External;
using HappyTravel.Edo.Api.Services.Payments.NGenius;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/external/payments")]
    public class ExternalPaymentsController : BaseController
    {
        public ExternalPaymentsController(IPaymentCallbackDispatcher callbackDispatcher, NGeniusWebhookProcessingService nGeniusWebhookProcessingService)
        {
            _callbackDispatcher = callbackDispatcher;
            _nGeniusWebhookProcessingService = nGeniusWebhookProcessingService;
        }


        /// <summary>
        ///     Processes payment callback
        /// </summary>
        [AllowAnonymous]
        [HttpPost("callback")]
        [ProducesResponseType(typeof(PaymentResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PaymentCallback([FromForm] IFormCollection form)
        {
            if (form is null)
                return BadRequest(ProblemDetailsBuilder.Build("Payment data is required"));

            var dictionary = form.ToDictionary(k => k.Key, k => WebUtility.UrlDecode(k.Value.ToString()));
            var value = JObject.FromObject(dictionary);
            return OkOrBadRequest(await _callbackDispatcher.ProcessCallback(value));
        }
        
        
        /// <summary>
        ///     NGenius webhook
        /// </summary>
        [HttpPost("ngenius/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> NGeniusWebhook(NGeniusWebhookRequest request)
        {
            await _nGeniusWebhookProcessingService.ProcessWebHook(request);
            return Ok();
        }


        private readonly IPaymentCallbackDispatcher _callbackDispatcher;
        private readonly NGeniusWebhookProcessingService _nGeniusWebhookProcessingService;
    }
}