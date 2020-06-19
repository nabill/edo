using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Filters.Authorization.CounterpartyStatesFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
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
        ///     Creates agency for counterparty.
        /// </summary>
        /// <param name="counterpartyId">Counterparty Id.</param>
        /// <param name="agencyInfo">Agency information.</param>
        /// <returns></returns>
        [HttpPost("{counterpartyId}/agencies")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AgentRequired]
        public async Task<IActionResult> AddAgency(int counterpartyId, [FromBody] AgencyInfo agencyInfo)
        {
            var (isSuccess, _, _, error) = await _counterpartyService.AddAgency(counterpartyId, agencyInfo);

            return isSuccess
                ? (IActionResult) NoContent()
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        ///     Gets agency.
        /// </summary>
        /// <param name="counterpartyId">Counterparty Id.</param>
        /// <param name="agencyId">Agency Id.</param>
        /// <returns></returns>
        [HttpGet("{counterpartyId}/agencies/{agencyId}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAgency(int counterpartyId, int agencyId)
        {
            var (_, isFailure, agency, error) = await _counterpartyService.GetAgency(agencyId);

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
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
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
        [InAgencyPermissions(InAgencyPermissions.EditCounterpartyInfo)]
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
        [ProducesResponseType(typeof(CounterpartyInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
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