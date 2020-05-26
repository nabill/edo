using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Users
{
    public interface IUserInvitationService
    {
        Task<Result> Send<TInvitationData>(string email, TInvitationData invitationInfo,
            Func<TInvitationData, string, DataWithCompanyInfo> messagePayloadGenerator, string mailTemplateId, UserInvitationTypes invitationType);


        Task<Result<string>> Create<TInvitationData>(string email, TInvitationData invitationInfo, UserInvitationTypes invitationType);

        Task Accept(string invitationCode);

        Task<Result<TInvitationData>> GetPendingInvitation<TInvitationData>(string invitationCode, UserInvitationTypes invitationType);
    }
}