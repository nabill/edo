using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentInvitationService
    {
        Task<Result> Send(SendAgentInvitationRequest sendAgentInvitationRequest);

        Task Accept(string invitationCode);

        Task<Result<AgentInvitationInfo>> GetPendingInvitation(string invitationCode);

        Task<Result<string>> Create(SendAgentInvitationRequest sendAgentInvitationRequest);

        Task<List<AgentInvitationInfo>> GetAgencyInvitations(int agencyId);

        Task<List<AgentInvitationInfo>> GetAgentInvitations(int agencyId);
    }
}