using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/management")]
    [Produces("application/json")]
    public class ManagementController : BaseController
    {
        private readonly IAdministratorInvitationService _invitationService;

        public ManagementController(IAdministratorInvitationService invitationService)
        {
            _invitationService = invitationService;
        }

        /// <summary>
        ///     Send invitation to administrator.
        /// </summary>
        /// <param name="invitationInfo">Administrator invitation info.</param>
        /// <returns></returns>
        [HttpPost("inviteAdmin")]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        public async Task<IActionResult> InviteAdministrator([FromBody]AdministratorInvitationInfo invitationInfo)
        {
            var (_, isFailure, error) = await _invitationService.SendInvitation(invitationInfo);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
    }
}