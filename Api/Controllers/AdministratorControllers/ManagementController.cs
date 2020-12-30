using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Services.Management;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/management")]
    [Produces("application/json")]
    public class ManagementController : BaseController
    {
        public ManagementController(IAdministratorInvitationService invitationService,
            IAdministratorRegistrationService registrationService,
            ITokenInfoAccessor tokenInfoAccessor,
            IAgentMovementService agentMovementService)
        {
            _invitationService = invitationService;
            _registrationService = registrationService;
            _tokenInfoAccessor = tokenInfoAccessor;
            _agentMovementService = agentMovementService;
        }


        /// <summary>
        ///     Send invitation to administrator.
        /// </summary>
        /// <param name="invitationInfo">Administrator invitation info.</param>
        /// <returns></returns>
        [HttpPost("invite")]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [AdministratorPermissions(AdministratorPermissions.AdministratorInvitation)]
        public async Task<IActionResult> InviteAdministrator([FromBody] AdministratorInvitationInfo invitationInfo)
        {
            var (_, isFailure, error) = await _invitationService.SendInvitation(invitationInfo);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Register current user as administrator by invitation code.
        /// </summary>
        /// <param name="invitationCode">Invitation code.</param>
        /// <returns></returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> RegisterAdministrator([FromBody] string invitationCode)
        {
            var identity = _tokenInfoAccessor.GetIdentity();
            if (string.IsNullOrWhiteSpace(identity))
                return BadRequest(ProblemDetailsBuilder.Build("Could not get user's identity"));

            var (_, isFailure, error) = await _registrationService.RegisterByInvitation(invitationCode, identity);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        
        /// <summary>
        /// Move agent from one agency to another
        /// <param name="request">Change agent agency request</param>
        /// </summary>
        [HttpPost("change-agent-agency")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType((int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> MoveAgentToAgency([FromBody] ChangeAgentAgencyRequest request)
        {
            var (_, isFailure, error) = await _agentMovementService.Move(request.AgentId, request.SourceAgencyId, request.DestinationAgencyId);
            if (isFailure)
                return BadRequest(error);
            
            return Ok();
        }


        private readonly IAdministratorInvitationService _invitationService;
        private readonly IAdministratorRegistrationService _registrationService;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
        private readonly IAgentMovementService _agentMovementService;
    }
}