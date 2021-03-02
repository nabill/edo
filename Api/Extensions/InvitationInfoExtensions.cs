using System;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Invitations;
using HappyTravel.Edo.Api.Models.Management;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class InvitationInfoExtensions
    {
        public static UserInvitationData ToUserInvitationData(this AdministratorInvitationInfo info)
            => new UserInvitationData(new AgentEditableInfo(info.Title, info.FirstName, info.LastName, info.Position, info.Email), default);


        public static UserInvitationData ToUserInvitationData(this AgentEditableInfo info, string email = null)
        {
            var newInfo = new AgentEditableInfo(info.Title, info.FirstName, info.LastName, info.Position, email ?? info.Email);
            return new UserInvitationData(newInfo, default);
        }
    }
}
