using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Filters.Authorization.CounterpartyStatesFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Services.AdministratorServices;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/counterparties")]
    [Produces("application/json")]
    public class CounterpartiesController : BaseController
    {
        public CounterpartiesController(ICounterpartyManagementService counterpartyManagementService)
        {
            _counterpartyManagementService = counterpartyManagementService;
        }


        /// <summary>
        /// Gets specified counterparty.
        /// </summary>
        /// <param name="counterpartyId">Id of counterparty to get</param>
        /// <returns></returns>
        [HttpGet("{counterpartyId}")]
        [ProducesResponseType(typeof(List<CounterpartyInfo>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyManagement)]
        public async Task<IActionResult> Get(int counterpartyId)
        {
            var (_, isFailure, counterparties, error) = await _counterpartyManagementService.Get(counterpartyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(counterparties);
        }


        /// <summary>
        /// Gets all counterparties
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<CounterpartyInfo>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyManagement)]
        public async Task<IActionResult> Get() => Ok(await _counterpartyManagementService.Get());


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
            var (isSuccess, _, error) = await _counterpartyManagementService.VerifyAsFullyAccessed(counterpartyId, request.Reason);

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
            var (isSuccess, _, error) = await _counterpartyManagementService.VerifyAsReadOnly(counterpartyId, request.Reason);

            return isSuccess
                ? (IActionResult) NoContent()
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        ///     Gets all agencies of a counterparty.
        /// </summary>
        /// <param name="counterpartyId">Counterparty Id.</param>
        /// <returns></returns>
        [HttpGet("{counterpartyId}/agencies")]
        [ProducesResponseType(typeof(List<AgencyInfo>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyManagement)]
        public async Task<IActionResult> GetAgencies(int counterpartyId)
        {
            var (_, isFailure, agencies, error) = await _counterpartyManagementService.GetAllCounterpartyAgencies(counterpartyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(agencies);
        }


        /// <summary>
        ///     Updates counterparty information.
        /// </summary>
        /// <param name="counterpartyId">Id of the counterparty.</param>
        /// <param name="updatedCounterpartyInfo">New counterparty information.</param>
        /// <returns></returns>
        [HttpPut("{counterpartyId}")]
        [ProducesResponseType(typeof(CounterpartyInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.ReadOnly)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyManagement)]
        public async Task<IActionResult> UpdateCounterparty(int counterpartyId, [FromBody] CounterpartyInfo updatedCounterpartyInfo)
        {
            var (_, isFailure, savedCounterpartyInfo, error) = await _counterpartyManagementService.Update(updatedCounterpartyInfo, counterpartyId);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(savedCounterpartyInfo);
        }


        private readonly ICounterpartyManagementService _counterpartyManagementService;
    }
}