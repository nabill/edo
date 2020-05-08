using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Filters.Authorization.CounterpartyStatesFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InCounterpartyPermissionFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/counterparties")]
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


        /// <summary>
        ///     Creates agency for counterparty.
        /// </summary>
        /// <param name="counterpartyId">Counterparty Id.</param>
        /// <param name="agencyInfo">Agency information.</param>
        /// <returns></returns>
        [HttpPost("{counterpartyId}/agencies")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AgentRequired]
        public async Task<IActionResult> AddAgency(int counterpartyId, [FromBody] AgencyInfo agencyInfo)
        {
            var (isSuccess, _, _, error) = await _counterpartyService.AddAgency(counterpartyId, agencyInfo);

            return isSuccess
                ? (IActionResult)NoContent()
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        ///     Gets agency.
        /// </summary>
        /// <param name="counterpartyId">Counterparty Id.</param>
        /// <param name="agencyId">Agency Id.</param>
        /// <returns></returns>
        [HttpGet("{counterpartyId}/agencies/{agencyId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAgency(int counterpartyId, int agencyId)
        {
            var (_, isFailure, agency, error) = await _counterpartyService.GetAgency(counterpartyId, agencyId);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(agency);
        }


        /// <summary>
        ///     Gets all agencies of a counterparty.
        /// </summary>
        /// <param name="counterpartyId">Counterparty Id.</param>
        /// <returns></returns>
        [HttpGet("{counterpartyId}/agencies")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAgencies(int counterpartyId)
        {
            var (_, isFailure, agency, error) = await _counterpartyService.GetAllCounterpartyAgencies(counterpartyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(agency);
        }


        /// <summary>
        ///     Updates counterparty information.
        /// </summary>
        /// <param name="counterpartyId">Id of the counterparty to verify.</param>
        /// <param name="updatedCounterpartyInfo">New counterparty information.</param>
        /// <returns></returns>
        [HttpPut("{counterpartyId}")]
        [ProducesResponseType(typeof(CounterpartyInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.ReadOnly)]
        [InCounterpartyPermissions(InCounterpartyPermissions.EditCounterpartyInfo)]
        public async Task<IActionResult> UpdateCounterparty(int counterpartyId, [FromBody] CounterpartyInfo updatedCounterpartyInfo)
        {
            var (_, isFailure, savedCounterpartyInfo, error) = await _counterpartyService.Update(updatedCounterpartyInfo, counterpartyId);
            
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(savedCounterpartyInfo);
        }

        /// <summary>
        ///     Gets counterparty information.
        /// </summary>
        /// <param name="counterpartyId">Id of the counterparty to verify.</param>
        /// <returns></returns>
        [HttpGet("{counterpartyId}")]
        [ProducesResponseType(typeof(CounterpartyInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetCounterparty(int counterpartyId)
        {
            var (_, isFailure, counterpartyInfo, error) = await _counterpartyService.Get(counterpartyId);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(counterpartyInfo);
        }

        private readonly ICounterpartyService _counterpartyService;
    }
}