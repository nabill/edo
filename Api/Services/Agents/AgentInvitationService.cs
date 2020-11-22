using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Services.Users;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgentInvitationService : IAgentInvitationService
    {
        public AgentInvitationService(IOptions<AgentInvitationOptions> options,
            IUserInvitationService invitationService,
            ICounterpartyService counterpartyService,
            EdoContext context,
            IAgentContextService agentContextService)
        {
            _invitationService = invitationService;
            _counterpartyService = counterpartyService;
            _options = options.Value;
            _context = context;
            _agentContextService = agentContextService;
        }


        public async Task<Result> Send(SendAgentInvitationRequest request, AgentContext agent)
        {
            var agencyName = (await _counterpartyService.GetAgency(agent.AgencyId, agent)).Value.Name;

            var messagePayloadGenerator = new Func<AgentInvitationInfo, string, DataWithCompanyInfo>((info, invitationCode) => new AgentInvitationData
            {
                AgencyName = agencyName,
                InvitationCode = invitationCode,
                UserEmailAddress = info.Email,
                UserName = $"{info.RegistrationInfo.FirstName} {info.RegistrationInfo.LastName}"
            });

            var invitationInfo = new AgentInvitationInfo(new AgentEditableInfo(
                request.RegistrationInfo.Title,
                request.RegistrationInfo.FirstName,
                request.RegistrationInfo.LastName,
                request.RegistrationInfo.Position,
                request.Email),
                agent.AgencyId, agent.AgentId, request.Email);

            return await _invitationService.Send(request.Email, invitationInfo, messagePayloadGenerator,
                _options.MailTemplateId, UserInvitationTypes.Agent);
        }


        public async Task<Result<string>> Create(SendAgentInvitationRequest request, AgentContext agent)
        {
            var invitationInfo = new AgentInvitationInfo(new AgentEditableInfo(
                    request.RegistrationInfo.Title,
                    request.RegistrationInfo.FirstName,
                    request.RegistrationInfo.LastName,
                    request.RegistrationInfo.Position,
                    request.Email),
                agent.AgencyId, agent.AgentId, request.Email);

            return await _invitationService.Create(invitationInfo.Email, invitationInfo, UserInvitationTypes.Agent);
        }


        public Task Accept(string invitationCode) => _invitationService.Accept(invitationCode);


        public Task<Result<AgentInvitationInfo>> GetPendingInvitation(string invitationCode)
            => _invitationService.GetPendingInvitation<AgentInvitationInfo>(invitationCode, UserInvitationTypes.Agent);


        public Task<List<AgentInvitationResponse>> GetAgentInvitations(int agentId)
        {
            return _context
                .AgentInvitations
                .Where(i => i.Data.AgentId == agentId && !i.IsResent)
                .Select(i => new AgentInvitationResponse(i.CodeHash, i.Data.RegistrationInfo.Title, i.Data.RegistrationInfo.FirstName,
                    i.Data.RegistrationInfo.LastName, i.Data.RegistrationInfo.Position, i.Email))
                .ToListAsync();
        }


        public Task<List<AgentInvitationResponse>> GetAgencyInvitations(int agencyId)
        {
            return _context
                .AgentInvitations
                .Where(i => i.Data.AgencyId == agencyId && !i.IsResent)
                .Select(i => new AgentInvitationResponse(i.CodeHash, i.Data.RegistrationInfo.Title, i.Data.RegistrationInfo.FirstName,
                        i.Data.RegistrationInfo.LastName, i.Data.RegistrationInfo.Position, i.Email))
                .ToListAsync();
        }


        public async Task<Result<(SendAgentInvitationRequest NewInvitation, AgentInvitation OldInvitation), ProblemDetails>> ReSend(string invitationId)
        {
            var agent = await _agentContextService.GetAgent();

            return await GetExistedInvitation()
                .Bind(CreateNewInvitation)
                .Bind(SendInvitation)
                .Bind(DisableOldInvitation);


            async Task<Result<AgentInvitation, ProblemDetails>> GetExistedInvitation()
            {
                var invitation = await _context
                    .AgentInvitations
                    .SingleOrDefaultAsync(i => i.CodeHash == invitationId);

                return invitation ?? ProblemDetailsBuilder.Fail<AgentInvitation>($"Invitation with Id {invitationId} not found");
            }


            async Task<Result<(SendAgentInvitationRequest NewInvitation, AgentInvitation OldInvitation), ProblemDetails>> CreateNewInvitation(AgentInvitation existedInvitation)
            {
                var newInvitation = new SendAgentInvitationRequest(new AgentEditableInfo(
                    existedInvitation.Data.RegistrationInfo.Title,
                    existedInvitation.Data.RegistrationInfo.FirstName,
                    existedInvitation.Data.RegistrationInfo.LastName,
                    existedInvitation.Data.RegistrationInfo.Position,
                    existedInvitation.Email), existedInvitation.Email);

                var (_, isFailure, _, error) = await Create(newInvitation, agent);
                return isFailure
                    ? ProblemDetailsBuilder.Fail<(SendAgentInvitationRequest NewInvitation, AgentInvitation OldInvitation)>(error)
                    : (NewInvitation: newInvitation, OldInvitation: existedInvitation);
            }


            async Task<Result<(SendAgentInvitationRequest NewInvitation, AgentInvitation OldInvitation), ProblemDetails>> SendInvitation((SendAgentInvitationRequest NewInvitation, AgentInvitation OldInvitation) invitations)
            {
                var (_, isFailure, error) = await Send(invitations.NewInvitation, agent);

                return isFailure
                    ? ProblemDetailsBuilder.Fail<(SendAgentInvitationRequest NewInvitation, AgentInvitation OldInvitation)>(error)
                    : invitations;
            }


            async Task<Result<(SendAgentInvitationRequest NewInvitation, AgentInvitation OldInvitation), ProblemDetails>> DisableOldInvitation((SendAgentInvitationRequest NewInvitation, AgentInvitation OldInvitation) invitations)
            {
                if (invitations.OldInvitation is null)
                {
                    return ProblemDetailsBuilder.Fail<(SendAgentInvitationRequest NewInvitation, AgentInvitation OldInvitation)>("Old invitation can not be null");
                }

                invitations.OldInvitation.IsResent = true;
                await _context.SaveChangesAsync();
                return invitations;
            }
        }


        private readonly IUserInvitationService _invitationService;
        private readonly ICounterpartyService _counterpartyService;
        private readonly AgentInvitationOptions _options;
        private readonly EdoContext _context;
        private readonly IAgentContextService _agentContextService;
    }
}