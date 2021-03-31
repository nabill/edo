using System;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Invitations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Invitations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}")]
    [Produces("application/json")]
    public class InvitationsController : BaseController
    {

        public InvitationsController(IInvitationRecordService invitationRecordService)
        {
            _invitationRecordService = invitationRecordService;
        }


        /// <summary>
        ///     Gets invitation data.
        /// </summary>
        /// <param name="code">Invitation code.</param>
        /// <returns>Invitation data, including pre-filled registration information.</returns>
        [HttpGet("invitations/{code}")]
        [ProducesResponseType(typeof(AgentInvitationInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetInvitationData(string code)
        {
            var (_, isFailure, invitation, error) = await _invitationRecordService
                .GetActiveInvitationByCode(code);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            if (!invitation.InviterAgencyId.HasValue)
                return BadRequest(ProblemDetailsBuilder.Build("Could not get agent invitation"));

            var data = JsonConvert.DeserializeObject<UserInvitationData>(invitation.Data);

            return Ok(new AgentInvitationInfo(data.UserRegistrationInfo,
                data.ChildAgencyRegistrationInfo,
                invitation.InvitationType,
                invitation.InviterAgencyId.Value,
                invitation.InviterUserId,
                data.UserRegistrationInfo.Email));
        }


        private readonly IInvitationRecordService _invitationRecordService;
    }
}
