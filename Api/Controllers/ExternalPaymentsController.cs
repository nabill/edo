using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/external/payments")]
    [Produces("application/json")]
    public class ExternalPaymentsController : BaseController
    {
        public ExternalPaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        ///     Processes payment callback
        /// </summary>
        [AllowAnonymous]
        [HttpPost("callback")]
        [ProducesResponseType(typeof(PaymentResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PaymentCallback([FromBody]JObject value)
        {
            return OkOrBadRequest(await _paymentService.ProcessPaymentResponse(value));
        }

        private readonly IPaymentService _paymentService;
    }
}