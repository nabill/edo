using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Services.Users;
using HappyTravel.Edo.Common.Enums;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Management
{
    public class AdministratorInvitationService : IAdministratorInvitationService
    {
        public AdministratorInvitationService(IUserInvitationService userInvitationService,
            IOptions<AdministratorInvitationOptions> options)
        {
            _userInvitationService = userInvitationService;
            _options = options.Value;
        }


        public Task<Result> SendInvitation(AdministratorInvitationInfo invitationInfo)
        {
            var messagePayloadGenerator = new Func<AdministratorInvitationInfo, string, DataWithCompanyInfo>((info, invitationCode) => new AdministratorInvitationData
            {
                InvitationCode = invitationCode,
                UserEmailAddress = invitationInfo.Email,
                UserName = $"{invitationInfo.FirstName} {invitationInfo.LastName}"
            });

            return _userInvitationService.Send(invitationInfo.Email, invitationInfo, messagePayloadGenerator, _options.MailTemplateId,
                UserInvitationTypes.Administrator);
        }


        public Task AcceptInvitation(string invitationCode) => _userInvitationService.Accept(invitationCode);


        public Task<Result<AdministratorInvitationInfo>> GetPendingInvitation(string invitationCode)
            => _userInvitationService
                .GetPendingInvitation<AdministratorInvitationInfo>(invitationCode, UserInvitationTypes.Administrator);


        private readonly AdministratorInvitationOptions _options;
        private readonly IUserInvitationService _userInvitationService;
    }
}