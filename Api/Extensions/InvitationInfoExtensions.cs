using System;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Invitations;
using HappyTravel.Edo.Api.Models.Users;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class InvitationInfoExtensions
    {
        public static UserInvitationData ToUserInvitationData(this UserDescriptionInfo info, string email = null)
        {
            var newInfo = new UserDescriptionInfo(info.Title, info.FirstName, info.LastName, info.Position, email ?? info.Email);
            return new UserInvitationData(newInfo, default);
        }


        public static UserInvitationData ToUserInvitationData(this RegisterInvitedAgencyRequest request)
            => new UserInvitationData(request.RegistrationInfo, request.ChildAgencyRegistrationInfo);
    }
}
