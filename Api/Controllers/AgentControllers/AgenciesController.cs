using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}")]
    [Produces("application/json")]
    public class AgenciesController : BaseController
    {
        public AgenciesController(IAgencyService agencyService,
            IAgentContextService agentContextService)
        {
            _agencyService = agencyService;
            _agentContextService = agentContextService;
        }


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
            var (_, isFailure, agency, error) = await _agencyService.GetAgency(agencyId, await _agentContextService.GetAgent());

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(agency);
        }


        /// <summary>
        ///     Gets child agencies.
        /// </summary>
        /// <returns></returns>
        [HttpGet("agency/child-agencies")]
        [ProducesResponseType(typeof(List<AgencyInfo>), (int)HttpStatusCode.OK)]
        [InAgencyPermissions(InAgencyPermissions.ObserveChildAgencies)]
        public async Task<IActionResult> GetChildAgencies()
            => Ok(await _agencyService.GetChildAgencies(await _agentContextService.GetAgent()));


        private readonly IAgencyService _agencyService;
        private readonly IAgentContextService _agentContextService;
    }
}
