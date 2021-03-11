using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Invitations;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Invitations;
using HappyTravel.Edo.Api.Services;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Invitations;
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
            IAgentContextService agentContextService,
            IAgentInvitationCreateService agentInvitationCreateService,
            IAgencyManagementService agencyManagementService)
        {
            _agencyService = agencyService;
            _agentContextService = agentContextService;
            _agentInvitationCreateService = agentInvitationCreateService;
            _agencyManagementService = agencyManagementService;
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


        /// <summary>
        ///     Invites to create child agency.
        /// </summary>
        /// <returns></returns>
        [HttpPost("agency/invitations/send")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [InAgencyPermissions(InAgencyPermissions.InviteChildAgencies)]
        public async Task<IActionResult> InviteChildAgency([FromBody] UserInvitationData request)
        {
            var agent = await _agentContextService.GetAgent();
            var (_, isFailure, code, error) = await _agentInvitationCreateService.Send(request,
                UserInvitationTypes.ChildAgency, agent.AgentId, agent.AgencyId);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(code);
        }


        /// <summary>
        ///     Invites to create child agency.
        /// </summary>
        /// <returns></returns>
        [HttpPost("agency/invitations/generate")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [InAgencyPermissions(InAgencyPermissions.InviteChildAgencies)]
        public async Task<IActionResult> GenerateChildAgencyInvite([FromBody] UserInvitationData request)
        {
            var agent = await _agentContextService.GetAgent();
            var (_, isFailure, code, error) = await _agentInvitationCreateService.Create(request,
                UserInvitationTypes.ChildAgency, agent.AgentId, agent.AgencyId);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(code);
        }
        
        
        /// <summary>
        ///  Deactivates specified agency.
        /// </summary>
        /// <param name="agencyId">Id of the agency.</param>
        /// <returns></returns>
        [HttpPost("agency/child-agencies/{agencyId}/deactivate")]
        [ProducesResponseType(typeof(CounterpartyInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DeactivateAgency(int agencyId)
        {
            var agent = await _agentContextService.GetAgent();
            var (_, isFailure, error) = await _agencyManagementService.DeactivateAgency(agencyId, agent);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///  Activates specified agency.
        /// </summary>
        /// <param name="agencyId">Id of the agency.</param>
        /// <returns></returns>
        [HttpPost("agency/child-agencies/{agencyId}/activate")]
        [ProducesResponseType(typeof(CounterpartyInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ActivateAgency(int agencyId)
        {
            var agent = await _agentContextService.GetAgent();
            var (_, isFailure, error) = await _agencyManagementService.ActivateAgency(agencyId, agent);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        private readonly IAgencyService _agencyService;
        private readonly IAgentContextService _agentContextService;
        private readonly IAgentInvitationCreateService _agentInvitationCreateService;
        private readonly IAgencyManagementService _agencyManagementService;
    }
}
