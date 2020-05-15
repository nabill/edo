using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Common.Enums;
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


        /// <summary>
        ///     Gets payment history for a current agent.
        /// </summary>
        /// <param name="counterpartyId">The agent could have relations with different counterparties</param>
        /// <param name="historyRequest"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<PaymentHistoryData>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AgentRequired]
        [HttpPost("history/{counterpartyId}/agent")]
        public async Task<IActionResult> GetAgentHistory([Required] int counterpartyId, [FromBody] PaymentHistoryRequest historyRequest)
        {
            var (_, isFailure, response, error) = await _paymentHistoryService.GetAgentHistory(historyRequest, counterpartyId);
            if (isFailure)
                return BadRequest(error);

            return Ok(response);
        }


        /// <summary>
        ///     Gets payment history for a counterparty.
        /// </summary>
        /// <param name="counterpartyId"></param>
        /// <param name="historyRequest"></param>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<PaymentHistoryData>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [HttpPost("history/{counterpartyId}")]
        [InAgencyPermissions(InAgencyPermissions.ViewCounterpartyAllPaymentHistory)]
        public async Task<IActionResult> GetCounterpartyHistory([Required] int counterpartyId, [FromBody] PaymentHistoryRequest historyRequest)
        {
            var (_, isFailure, response, error) = await _paymentHistoryService.GetCounterpartyHistory(historyRequest, counterpartyId);
            if (isFailure)
                return BadRequest(error);

            return Ok(response);
        }


        private readonly IPaymentHistoryService _paymentHistoryService;
    }
}