using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Common.Enums;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Models.Management
{
    public class AdministratorInvitationService : IAdministratorInvitationService
    {
        public AdministratorInvitationService(IUserInvitationService userInvitationService,
            IOptions<AdministratorInvitationOptions> options,
            IExternalAdminContext externalAdminContext)
        {
            _userInvitationService = userInvitationService;
            _externalAdminContext = externalAdminContext;
            _options = options.Value;
        }
        
        public Task<Result> SendInvitation(AdministratorInvitationInfo invitationInfo)
        {
            return _externalAdminContext.IsExternalAdmin()
                ? _userInvitationService.SendInvitation(invitationInfo.Email, invitationInfo, _options.MailTemplateId, UserInvitationTypes.Administrator)
                : Task.FromResult(Result.Fail("Only external admins can send invitations"));
        }

        public Task AcceptInvitation(string invitationCode)
        {
            return _userInvitationService.AcceptInvitation(invitationCode);
        }

        public Task<Result<AdministratorInvitationInfo>> GetPendingInvitation(string invitationCode)
        {
            return _userInvitationService
                .GetPendingInvitation<AdministratorInvitationInfo>(invitationCode, UserInvitationTypes.Administrator);
        }
        
        private readonly IUserInvitationService _userInvitationService;
        private readonly IExternalAdminContext _externalAdminContext;
        private readonly AdministratorInvitationOptions _options;
    }
}