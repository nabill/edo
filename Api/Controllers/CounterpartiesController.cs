using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
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


        ///// <summary>
        /////     Creates agency for counterparty.
        ///// </summary>
        ///// <param name="counterpartyId">Counterparty Id.</param>
        ///// <param name="agencyInfo">Agency information.</param>
        ///// <returns></returns>
        //[HttpPost("{counterpartyId}/agencies")]
        //[ProducesResponseType((int) HttpStatusCode.NoContent)]
        //[ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        //[AgentRequired]
        //public async Task<IActionResult> AddAgency(int counterpartyId, [FromBody] AgencyInfo agencyInfo)
        //{
        //    var (isSuccess, _, _, error) = await _counterpartyService.AddAgency(counterpartyId, agencyInfo);

        //    return isSuccess
        //        ? (IActionResult) NoContent()
        //        : BadRequest(ProblemDetailsBuilder.Build(error));
        //}


        /// <summary>
        ///     Gets agency.
        /// </summary>
        /// <param name="agencyId">Agency Id.</param>
        /// <returns></returns>
        [HttpGet("agencies/{agencyId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AgentRequired]
        public async Task<IActionResult> GetAgency(int agencyId)
        {
            var (_, isFailure, agency, error) = await _counterpartyService.GetAgency(agencyId);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(agency);
        }


        /// <summary>
        ///     Gets counterparty information.
        /// </summary>
        /// <param name="counterpartyId">Id of the counterparty.</param>
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