using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Invitations
{
    public interface IInvitationRecordsListService
    {
        Task<List<AgentInvitationResponse>> GetAgentAcceptedInvitations(int agentId);

        Task<List<AgentInvitationResponse>> GetAgentNotAcceptedInvitations(int agentId);

        Task<List<AgentInvitationResponse>> GetAgencyAcceptedInvitations(int agencyId);

        Task<List<AgentInvitationResponse>> GetAgencyNotAcceptedInvitations(int agencyId);
    }
}
