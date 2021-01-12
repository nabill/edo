using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public interface IAgentInvitationService
    {
        Task<Result> Send(SendAgentInvitationRequest sendAgentInvitationRequest, AgentContext agentContext);

        Task Accept(string invitationCode);

        Task<Result<AgentInvitationInfo>> GetPendingInvitation(string invitationCode);

        Task<Result<string>> Create(SendAgentInvitationRequest sendAgentInvitationRequest, AgentContext agentContext);

        Task<List<AgentInvitationResponse>> GetAgencyAcceptedInvitations(int agencyId);
        
        Task<List<AgentInvitationResponse>> GetAgencyNotAcceptedInvitations(int agencyId);

        Task<List<AgentInvitationResponse>> GetAgentAcceptedInvitations(int agencyId);
        Task<List<AgentInvitationResponse>> GetAgentNotAcceptedInvitations(int agencyId);

        Task<Result> Resend(string invitationId, AgentContext agent);

        Task<Result> Disable(string invitationCode);
    }
}