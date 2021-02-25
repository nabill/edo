using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Services.Users;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgentInvitationService : IAgentInvitationService
    {
        public AgentInvitationService(IOptions<AgentInvitationOptions> options,
            IUserInvitationService invitationService,
            IAgencyService agencyService,
            EdoContext context)
        {
            _invitationService = invitationService;
            _options = options.Value;
            _agencyService = agencyService;
            _context = context;
        }


        public async Task<Result> Send(SendAgentInvitationRequest request, AgentContext agent)
        {
            var agencyName = (await _agencyService.GetAgency(agent.AgencyId, agent)).Value.Name;

            var messagePayloadGenerator = new Func<AgentInvitationInfo, string, DataWithCompanyInfo>((info, invitationCode) => new AgentInvitationData
            {
                AgencyName = agencyName,
                InvitationCode = invitationCode,
                UserEmailAddress = info.Email,
                UserName = $"{info.RegistrationInfo.FirstName} {info.RegistrationInfo.LastName}"
            });

            return await _invitationService.Send(request.Email, request.ToAgentInvitationInfo(agent), messagePayloadGenerator,
                _options.MailTemplateId, UserInvitationTypes.Agent);
        }


        public async Task<Result<string>> Create(SendAgentInvitationRequest request, AgentContext agent)
        {
            var invitationInfo = request.ToAgentInvitationInfo(agent);
            return await _invitationService.Create(invitationInfo.Email, invitationInfo, UserInvitationTypes.Agent);
        }


        public Task Accept(string invitationCode) => _invitationService.Accept(invitationCode);


        public Task<Result<AgentInvitationInfo>> GetPendingInvitation(string invitationCode)
            => _invitationService.GetPendingInvitation<AgentInvitationInfo>(invitationCode, UserInvitationTypes.Agent);


        public Task<List<AgentInvitationResponse>> GetAgentAcceptedInvitations(int agentId)
            => GetInvitations(i => i.Data.AgentId == agentId && i.IsAccepted);
        
        
        public Task<List<AgentInvitationResponse>> GetAgentNotAcceptedInvitations(int agentId)
            => GetInvitations(i => i.Data.AgentId == agentId && !i.IsAccepted);


        public Task<List<AgentInvitationResponse>> GetAgencyAcceptedInvitations(int agencyId)
            => GetInvitations(i => i.Data.AgencyId == agencyId && i.IsAccepted);
        
        
        public Task<List<AgentInvitationResponse>> GetAgencyNotAcceptedInvitations(int agencyId)
            => GetInvitations(i => i.Data.AgencyId == agencyId && !i.IsAccepted);


        public async Task<Result> Resend(string invitationCode, AgentContext agent)
        {
            return await GetExistingInvitation()
                .Bind(SendInvitation)
                .Tap(DisableExistingInvitation);


            async Task<Result<AgentInvitation>> GetExistingInvitation()
            {
                var invitation = await _context
                    .AgentInvitations
                    .SingleOrDefaultAsync(i => i.CodeHash == invitationCode);

                if (invitation is null)
                    return Result.Failure<AgentInvitation>($"Invitation with Code {invitationCode} not found");

                if (invitation.IsResent)
                    return Result.Failure<AgentInvitation>($"Already resent invitation");

                return invitation;
            }


            Task<Result<AgentInvitation>> SendInvitation(AgentInvitation existingInvitation)
                => Send(existingInvitation.ToSendAgentInvitationRequest(), agent).Map(() => existingInvitation);


            async Task DisableExistingInvitation(AgentInvitation existingInvitation)
            {
                existingInvitation.IsResent = true;
                _context.Update(existingInvitation);
                await _context.SaveChangesAsync();
            }
        }


        public Task<Result> Disable(string invitationCode) 
            => _invitationService.Disable(invitationCode);


        private Task<List<AgentInvitationResponse>> GetInvitations(Expression<Func<AgentInvitation, bool>> filterExpression)
        {
            return _context
                .AgentInvitations
                .NotResent()
                .Where(filterExpression)
                .Join(
                    _context.Agents,
                    invite => invite.Data.AgentId,
                    agent => agent.Id,
                    (invite, agent) => new { Invite = invite, Agent = agent }
                )
                .Select(i => new AgentInvitationResponse(
                    i.Invite.CodeHash,
                    i.Invite.Data.RegistrationInfo.Title, 
                    i.Invite.Data.RegistrationInfo.FirstName, 
                    i.Invite.Data.RegistrationInfo.LastName, 
                    i.Invite.Data.RegistrationInfo.Position, 
                    i.Invite.Email,
                    $"{i.Agent.FirstName} {i.Agent.LastName}",
                    DateTimeFormatters.ToDateString(i.Invite.Created))
                )
                .ToListAsync();
        }


        private readonly IUserInvitationService _invitationService;
        private readonly AgentInvitationOptions _options;
        private readonly IAgencyService _agencyService;
        private readonly EdoContext _context;
    }
}