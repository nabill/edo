using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Services.Users;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgentInvitationService : IAgentInvitationService
    {
        public AgentInvitationService(IAgentContextService agentContextService,
            IOptions<AgentInvitationOptions> options,
            IUserInvitationService invitationService,
            ICounterpartyService counterpartyService,
            EdoContext context)
        {
            _agentContextService = agentContextService;
            _invitationService = invitationService;
            _counterpartyService = counterpartyService;
            _options = options.Value;
            _context = context;
        }


        public async Task<Result> Send(SendAgentInvitationRequest request)
        {
            var agent = await _agentContextService.GetAgent();

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


        public async Task<Result<string>> Create(SendAgentInvitationRequest request)
        {
            var (agentId, _, agencyId, _) = await _agentContextService.GetAgent();
            var invitationInfo = new AgentInvitationInfo(new AgentEditableInfo(
                    request.RegistrationInfo.Title,
                    request.RegistrationInfo.FirstName,
                    request.RegistrationInfo.LastName,
                    request.RegistrationInfo.Position,
                    request.Email),
                agencyId, agentId, request.Email);

            return await _invitationService.Create(invitationInfo.Email, invitationInfo, UserInvitationTypes.Agent);
        }


        public Task Accept(string invitationCode) => _invitationService.Accept(invitationCode);


        public Task<Result<AgentInvitationInfo>> GetPendingInvitation(string invitationCode)
            => _invitationService.GetPendingInvitation<AgentInvitationInfo>(invitationCode, UserInvitationTypes.Agent);


        public Task<List<AgentInvitationInfo>> GetAgentInvitations(int agentId)
        {
            return _context
                .AgentInvitations
                .Where(i => i.Data.AgentId == agentId)
                .Select(i => new AgentInvitationInfo(new AgentEditableInfo(
                    i.Data.RegistrationInfo.Title,
                    i.Data.RegistrationInfo.FirstName,
                    i.Data.RegistrationInfo.LastName,
                    i.Data.RegistrationInfo.Position,
                    i.Email), i.Data.AgencyId, i.Data.AgentId, i.Email))
                .ToListAsync();
        }


        public Task<List<AgentInvitationInfo>> GetAgencyInvitations(int agencyId)
        {
            return _context
                .AgentInvitations
                .Where(i => i.Data.AgencyId == agencyId)
                .Select(i => new AgentInvitationInfo(new AgentEditableInfo(
                    i.Data.RegistrationInfo.Title,
                    i.Data.RegistrationInfo.FirstName,
                    i.Data.RegistrationInfo.LastName,
                    i.Data.RegistrationInfo.Position,
                    i.Email), i.Data.AgencyId, i.Data.AgentId, i.Email))
                .ToListAsync();
        }


        private readonly IAgentContextService _agentContextService;
        private readonly IUserInvitationService _invitationService;
        private readonly ICounterpartyService _counterpartyService;
        private readonly AgentInvitationOptions _options;
        private readonly EdoContext _context;
    }
}