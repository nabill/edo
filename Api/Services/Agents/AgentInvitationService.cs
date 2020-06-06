using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Services.Users;
using HappyTravel.Edo.Common.Enums;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgentInvitationService : IAgentInvitationService
    {
        public AgentInvitationService(IAgentContext agentContext,
            IOptions<AgentInvitationOptions> options,
            IUserInvitationService invitationService,
            ICounterpartyService counterpartyService)
        {
            _agentContext = agentContext;
            _invitationService = invitationService;
            _counterpartyService = counterpartyService;
            _options = options.Value;
        }


        public async Task<Result> Send(AgentInvitationInfo invitationInfo)
        {
            var agentAgencyId = (await _agentContext.GetAgent()).AgencyId;

            if (agentAgencyId != invitationInfo.AgencyId)
                return Result.Failure("Invitations can be send within an agency only");

            var agencyName = (await _counterpartyService.GetAgency(agentAgencyId)).Value.Name;

            var messagePayloadGenerator = new Func<AgentInvitationInfo, string, DataWithCompanyInfo>((info, invitationCode) => new AgentInvitationData
            {
                AgencyName = agencyName,
                InvitationCode = invitationCode,
                UserEmailAddress = info.Email,
                UserName = $"{info.RegistrationInfo.FirstName} {info.RegistrationInfo.LastName}"
            });

            return await _invitationService.Send(invitationInfo.Email, invitationInfo, messagePayloadGenerator,
                _options.MailTemplateId, UserInvitationTypes.Agent);
        }


        public async Task<Result<string>> Create(AgentInvitationInfo invitationInfo)
        {
            var agentAgencyId = (await _agentContext.GetAgent()).AgencyId;

            if (agentAgencyId != invitationInfo.AgencyId)
                return Result.Failure<string>("Invitations can be send within an agency only");
            
            return await _invitationService.Create(invitationInfo.Email, invitationInfo, UserInvitationTypes.Agent);
        }


        public Task Accept(string invitationCode) => _invitationService.Accept(invitationCode);


        public Task<Result<AgentInvitationInfo>> GetPendingInvitation(string invitationCode)
            => _invitationService.GetPendingInvitation<AgentInvitationInfo>(invitationCode, UserInvitationTypes.Agent);


        private readonly IAgentContext _agentContext;
        private readonly IUserInvitationService _invitationService;
        private readonly ICounterpartyService _counterpartyService;
        private readonly AgentInvitationOptions _options;
    }
}