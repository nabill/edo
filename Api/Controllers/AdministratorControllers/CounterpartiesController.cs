using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.AdministratorServices.Models;
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
            var (_, isFailure, counterparties, error) = await _counterpartyManagementService.Get(counterpartyId, LanguageCode);
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
        public async Task<IActionResult> Get() => Ok(await _counterpartyManagementService.Get(LanguageCode));


        /// <summary>
        ///     Sets counterparty fully verified.
        /// </summary>
        /// <param name="counterpartyId">Id of the counterparty to verify.</param>
        /// <param name="request">Verification details.</param>
        /// <returns></returns>
        [HttpPut("{counterpartyId}/verification-state")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyVerification)]
        public async Task<IActionResult> Verify(int counterpartyId, [FromBody] CounterpartyVerificationRequest request)
        {
            var (isSuccess, _, error) = await _counterpartyManagementService.Verify(counterpartyId, request.State, request.Reason);

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
        /// <param name="updateCounterpartyRequest">New counterparty information.</param>
        /// <returns></returns>
        [HttpPut("{counterpartyId}")]
        [ProducesResponseType(typeof(CounterpartyInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyManagement)]
        public async Task<IActionResult> UpdateCounterparty(int counterpartyId, [FromBody] CounterpartyEditRequest updateCounterpartyRequest)
        {
            var (_, isFailure, savedCounterpartyInfo, error) =
                await _counterpartyManagementService.Update(updateCounterpartyRequest, counterpartyId, LanguageCode);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(savedCounterpartyInfo);
        }


        /// <summary>
        ///  Changes specified counterparty activity status.
        /// </summary>
        /// <param name="counterpartyId">Id of the counterparty.</param>
        /// <param name="request">Request data for activity status change.</param>
        /// <returns></returns>
        [HttpPut("{counterpartyId}/activity-status/change")]
        [ProducesResponseType(typeof(CounterpartyInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyManagement)]
        public async Task<IActionResult> ChangeCounterpartyActivityStatus(int counterpartyId, ActivityStatusChangeRequest request)
        {
            var (_, isFailure, error) = await _counterpartyManagementService.ChangeCounterpartyActivityStatus(counterpartyId, request.Status, request.Reason);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///  Changes specified agency activity status.
        /// </summary>
        /// <param name="agencyId">Id of the agency.</param>
        /// <param name="request">Request data for activity status change.</param>
        /// <returns></returns>
        [HttpPut("agencies/{agencyId}/activity-status/change")]
        [ProducesResponseType(typeof(CounterpartyInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyManagement)]
        public async Task<IActionResult> DeactivateAgency(int agencyId, ActivityStatusChangeRequest request)
        {
            var (_, isFailure, error) = await _counterpartyManagementService.ChangeAgencyActivityStatus(agencyId, request.Status, request.Reason);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///  Returns counterparties predictions when searching
        /// </summary>
        /// <param name="query">The search query text.</param>
        /// <returns></returns>
        [HttpGet("predictions")]
        [ProducesResponseType(typeof(List<CounterpartyPrediction>), (int) HttpStatusCode.OK)]
        [AdministratorPermissions(AdministratorPermissions.PaymentLinkGeneration)]
        public async Task<IActionResult> GetCounterpartyPredictions(string query)
        {
            var result = await _counterpartyManagementService.GetCounterpartiesPredictions(query);
            return Ok(result);
        }


        private readonly ICounterpartyManagementService _counterpartyManagementService;
    }
}