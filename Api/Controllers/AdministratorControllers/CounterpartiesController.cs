using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Services.Agents;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/counterparties")]
    [Produces("application/json")]
    public class CounterpartiesController : BaseController
    {
        public CounterpartiesController(ICounterpartyService counterpartyService)
        {
            _counterpartyService = counterpartyService;
        }


        /// <summary>
        ///     Sets counterparty fully verified.
        /// </summary>
        /// <param name="counterpartyId">Id of the counterparty to verify.</param>
        /// <param name="request">Verification details.</param>
        /// <returns></returns>
        [HttpPost("{counterpartyId}/verify")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyVerification)]
        public async Task<IActionResult> Verify(int counterpartyId, [FromBody] CounterpartyVerificationRequest request)
        {
            var (isSuccess, _, error) = await _counterpartyService.VerifyAsFullyAccessed(counterpartyId, request.Reason);

            return isSuccess
                ? (IActionResult) NoContent()
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        ///     Sets counterparty read-only verified.
        /// </summary>
        /// <param name="counterpartyId">Id of the counterparty to verify.</param>
        /// <param name="request">Verification details.</param>
        /// <returns></returns>
        [HttpPost("{counterpartyId}/verify/read-only")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyVerification)]
        public async Task<IActionResult> VerifyAsReadOnly(int counterpartyId, [FromBody] CounterpartyVerificationRequest request)
        {
            var (isSuccess, _, error) = await _counterpartyService.VerifyAsReadOnly(counterpartyId, request.Reason);

            return isSuccess
                ? (IActionResult) NoContent()
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        private readonly ICounterpartyService _counterpartyService;
    }
}