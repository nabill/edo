using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Services.Invitations;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/management")]
    [Produces("application/json")]
    public class ManagementController : BaseController
    {
        public ManagementController(IInvitationService invitationService,
            ITokenInfoAccessor tokenInfoAccessor,
            IAdministratorContext administratorContext)
        {
            _invitationService = invitationService;
            _tokenInfoAccessor = tokenInfoAccessor;
            _administratorContext = administratorContext;
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
            var (_, _, admin, _) = await _administratorContext.GetCurrent();

            var (_, isFailure, error) = await _invitationService.Create(invitationInfo.ToUserInvitationData(),
                UserInvitationTypes.Administrator, true, admin.Id);

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

            var (_, isFailure, error) = await _invitationService.Accept(invitationCode, default, identity);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        
        /// <summary>
        ///     Disable invitation.
        /// </summary>
        /// <param name="code">Invitation code.</param>
        [HttpPost("invitations/{code}/disable")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DisableInvitation(string code)
        {
            var (_, isFailure, error) = await _invitationService.Disable(code);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok();
        }
        
        
        private readonly IInvitationService _invitationService;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
        private readonly IAdministratorContext _administratorContext;
    }
}