using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Invitations;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Invitations;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/invitations")]
    [Produces("application/json")]
    public class InvitationsController : BaseController
    {
        public InvitationsController(IInvitationRecordService invitationRecordService,
            IAdminInvitationAcceptService adminInvitationAcceptService,
            IAdminInvitationCreateService adminInvitationCreateService,
            ITokenInfoAccessor tokenInfoAccessor,
            IAdministratorContext administratorContext,
            IIdentityUserInfoService identityUserInfoService)
        {
            _invitationRecordService = invitationRecordService;
            _adminInvitationAcceptService = adminInvitationAcceptService;
            _adminInvitationCreateService = adminInvitationCreateService;
            _tokenInfoAccessor = tokenInfoAccessor;
            _administratorContext = administratorContext;
            _identityUserInfoService = identityUserInfoService;
        }


        /// <summary>
        ///     Send invitation to administrator.
        /// </summary>
        /// <param name="request">Administrator invitation request</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [AdministratorPermissions(AdministratorPermissions.AdministratorInvitation)]
        public async Task<IActionResult> InviteAdministrator([FromBody] SendAdminInvitationRequest request)
        {
            var (_, _, admin, _) = await _administratorContext.GetCurrent();

            var (_, isFailure, _, error) = await _adminInvitationCreateService.Send(request.ToUserInvitationData(), admin.Id);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Register current user as administrator by invitation code.
        /// </summary>
        /// <param name="invitationCode">Invitation code.</param>
        /// <returns></returns>
        [HttpPost("accept")]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> RegisterAdministrator([FromBody] string invitationCode)
        {
            var identity = _tokenInfoAccessor.GetIdentity();
            if (string.IsNullOrWhiteSpace(identity))
                return BadRequest(ProblemDetailsBuilder.Build("Could not get user's identity"));

            var email = await _identityUserInfoService.GetUserEmail();
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(ProblemDetailsBuilder.Build("E-mail claim is required"));

            var (_, isFailure, error) = await _adminInvitationAcceptService.Accept(invitationCode, identity, email);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        
        /// <summary>
        ///     Disable invitation.
        /// </summary>
        /// <param name="codeHash">Invitation code hash.</param>
        [HttpPost("{codeHash}/disable")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> DisableInvitation(string codeHash)
        {
            var (_, isFailure, error) = await _invitationRecordService.Revoke(codeHash);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok();
        }


        private readonly IInvitationRecordService _invitationRecordService;
        private readonly IAdminInvitationAcceptService _adminInvitationAcceptService;
        private readonly IAdminInvitationCreateService _adminInvitationCreateService;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
        private readonly IAdministratorContext _administratorContext;
        private readonly IIdentityUserInfoService _identityUserInfoService;
    }
}