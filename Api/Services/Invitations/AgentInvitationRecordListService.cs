using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Invitations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Infrastructure;
using HappyTravel.Formatters;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Invitations
{
    public class AgentInvitationRecordListService : IAgentInvitationRecordListService
    {
        public AgentInvitationRecordListService(
            EdoContext context,
            IInvitationRecordService invitationRecordService)
        {
            _context = context;
            _invitationRecordService = invitationRecordService;
        }


        public Task<List<AgentInvitationResponse>> GetAgentAcceptedInvitations(int agentId)
            => GetInvitationsWithInviter(i => i.InviterUserId == agentId && i.InvitationStatus == UserInvitationStatuses.Accepted);


        public Task<List<AgentInvitationResponse>> GetAgentNotAcceptedInvitations(int agentId)
            => GetInvitationsWithInviter(i => i.InviterUserId == agentId && i.InvitationStatus != UserInvitationStatuses.Accepted);


        public Task<List<AgentInvitationResponse>> GetAgencyAcceptedInvitations(int agencyId)
            => GetInvitationsWithInviter(i => i.InviterAgencyId == agencyId && i.InvitationStatus == UserInvitationStatuses.Accepted);


        public Task<List<AgentInvitationResponse>> GetAgencyNotAcceptedInvitations(int agencyId)
            => GetInvitationsWithInviter(i => i.InviterAgencyId == agencyId && i.InvitationStatus != UserInvitationStatuses.Accepted);


        private async Task<List<AgentInvitationResponse>> GetInvitationsWithInviter(Expression<Func<UserInvitation, bool>> filterExpression)
        {
            var rows = await _context
                .UserInvitations
                .Where(i => i.InvitationStatus != UserInvitationStatuses.Resent)
                .Where(filterExpression)
                .Join(
                    _context.Agents,
                    invite => invite.InviterUserId,
                    agent => agent.Id,
                    (invite, inviter) => new { Invite = invite, Inviter = inviter }
                )
                .ToListAsync();

            return rows.Select(r => new { r.Invite, r.Inviter, Data = _invitationRecordService.GetInvitationData(r.Invite) })
                .Select(i => new AgentInvitationResponse(
                    i.Invite.CodeHash,
                    i.Data.UserRegistrationInfo.Title,
                    i.Data.UserRegistrationInfo.FirstName,
                    i.Data.UserRegistrationInfo.LastName,
                    i.Data.UserRegistrationInfo.Position,
                    i.Invite.Email,
                    $"{i.Inviter.FirstName} {i.Inviter.LastName}",
                    DateTimeFormatters.ToDateString(i.Invite.Created),
                    i.Invite.InvitationStatus)
                )
                .ToList();
        }


        private readonly EdoContext _context;
        private readonly IInvitationRecordService _invitationRecordService;
    }
}
