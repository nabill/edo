using System.Linq;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Extensions
{
    public static class InvitationExtension
    {
        public static AgentInvitationInfo ToAgentInvitationInfo(this SendAgentInvitationRequest data, AgentContext agent)
        {
            return new AgentInvitationInfo(new AgentEditableInfo(
                    data.RegistrationInfo.Title,
                    data.RegistrationInfo.FirstName,
                    data.RegistrationInfo.LastName,
                    data.RegistrationInfo.Position,
                    data.Email),
                agent.AgencyId, agent.AgentId, data.Email);
        }


        public static SendAgentInvitationRequest ToSendAgentInvitationRequest(this AgentInvitation data)
        {
            return new SendAgentInvitationRequest(new AgentEditableInfo(
                data.Data.RegistrationInfo.Title,
                data.Data.RegistrationInfo.FirstName,
                data.Data.RegistrationInfo.LastName,
                data.Data.RegistrationInfo.Position,
                data.Email), data.Email);
        }


        public static IQueryable<AgentInvitation> NotResent(this IQueryable<AgentInvitation> queryable)
        {
            return queryable.Where(a => !a.IsResent);
        }
    }
}