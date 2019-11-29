using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Payments;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/payments")]
    [Produces("application/json")]
    public class PaymentsHistoryController : ControllerBase
    {
        public PaymentsHistoryController(IPaymentHistoryService paymentHistoryService)
        {
            _paymentHistoryService = paymentHistoryService;
        }

        [ProducesResponseType(typeof(List<PaymentHistoryData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [HttpGet("history/customer")]
        public async Task<IActionResult> GetCustomerHistory([FromBody] PaymentHistoryRequest historyRequest)
        {
            var (_, isFailure, response, error) = await _paymentHistoryService.GetCustomerHistory(historyRequest);
            if (isFailure)
                return BadRequest(error);

            return Ok(response);
        }


        [ProducesResponseType(typeof(List<PaymentHistoryData>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [HttpGet("history/company")]
        public async Task<IActionResult> GetCompanyHistory([FromBody] PaymentHistoryRequest historyRequest)
        {
            var (_, isFailure, response, error) = await _paymentHistoryService.GetCompanyHistory(historyRequest);
            if (isFailure)
                return BadRequest(error);

            return Ok(response);
        }


        private readonly IPaymentHistoryService _paymentHistoryService;
    }
}
