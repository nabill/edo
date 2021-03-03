using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Invitations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Services.Invitations
{
    public interface IInvitationRecordService
    {
        Task<Result<string>> Create(UserInvitationData prefilledData, UserInvitationTypes invitationType,
            bool shouldSendInvitationMail, int inviterUserId, int? inviterAgencyId = null);

        Task<Result> Revoke(string code);

        Task<Result<string>> Resend(string code);

        Task<Result> Accept(string code);

        Task<Result<UserInvitation>> GetActiveInvitation(string code);

        UserInvitationData GetInvitationData(UserInvitation invitation);

        Task<List<AgentInvitationResponse>> GetAgentAcceptedInvitations(int agentId);

        Task<List<AgentInvitationResponse>> GetAgentNotAcceptedInvitations(int agentId);

        Task<List<AgentInvitationResponse>> GetAgencyAcceptedInvitations(int agencyId);

        Task<List<AgentInvitationResponse>> GetAgencyNotAcceptedInvitations(int agencyId);
    }
}
