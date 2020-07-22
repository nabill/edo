using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.ServiceAccountFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Services.Management;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/management/admin")]
    [Produces("application/json")]
    public class ManagementController : BaseController
    {
        public ManagementController(IAdministratorInvitationService invitationService,
            IAdministratorRegistrationService registrationService,
            ITokenInfoAccessor tokenInfoAccessor)
        {
            _invitationService = invitationService;
            _registrationService = registrationService;
            _tokenInfoAccessor = tokenInfoAccessor;
        }


        /// <summary>
        ///     Send invitation to administrator.
        /// </summary>
        /// <param name="invitationInfo">Administrator invitation info.</param>
        /// <returns></returns>
        [HttpPost("invite")]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        // TODO: Allow administrator to invite another administrator
        [ServiceAccountRequired]
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


        private readonly IAdministratorInvitationService _invitationService;
        private readonly IAdministratorRegistrationService _registrationService;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
    }
}