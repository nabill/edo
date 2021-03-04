using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgentRegistrationService : IAgentRegistrationService
    {
        public AgentRegistrationService(EdoContext context,
            ICounterpartyService counterpartyService,
            IAgentService agentService,
            IOptions<AgentRegistrationNotificationOptions> notificationOptions,
            MailSenderWithCompanyInfo mailSender,
            ILogger<AgentRegistrationService> logger)
        {
            _context = context;
            _counterpartyService = counterpartyService;
            _agentService = agentService;
            _notificationOptions = notificationOptions.Value;
            _mailSender = mailSender;
            _logger = logger;
        }


        public Task<Result> RegisterWithCounterparty(UserDescriptionInfo agentData, CounterpartyEditRequest counterpartyData, string externalIdentity,
            string email)
        {
            return Result.Success()
                .Ensure(IsIdentityPresent, "User should have identity")
                .BindWithTransaction(_context, () => Result.Success()
                    .Bind(CreateCounterparty)
                    .Bind(CreateAgent)
                    .Tap(AddMasterAgentAgencyRelation))
                .Bind(LogSuccess)
                .Bind(SendRegistrationMailToAdmins)
                .OnFailure(LogFailure);

            bool IsIdentityPresent() => !string.IsNullOrWhiteSpace(externalIdentity);
            

            Task<Result<CounterpartyInfo>> CreateCounterparty() 
                => _counterpartyService.Add(counterpartyData);


            async Task<Result<(CounterpartyInfo, Agent)>> CreateAgent(CounterpartyInfo counterparty)
            {
                var (_, isFailure, agent, error) = await _agentService.Add(agentData, externalIdentity, email);
                return isFailure
                    ? Result.Failure<(CounterpartyInfo, Agent)>(error)
                    : Result.Success((counterparty1: counterparty, agent));
            }


            async Task AddMasterAgentAgencyRelation((CounterpartyInfo counterparty, Agent agent) counterpartyUserInfo)
            {
                var (counterparty, agent) = counterpartyUserInfo;
                var defaultAgency = await _counterpartyService.GetDefaultAgency(counterparty.Id);
                await AddAgentAgencyRelation(agent,
                    AgentAgencyRelationTypes.Master,
                    PermissionSets.Master,
                    defaultAgency.Id);
            }


            async Task<Result> SendRegistrationMailToAdmins(CounterpartyInfo counterpartyInfo)
            {
                var agent = $"{agentData.Title} {agentData.FirstName} {agentData.LastName}";
                if (!string.IsNullOrWhiteSpace(agentData.Position))
                    agent += $" ({agentData.Position})";

                var messageData = new RegistrationDataForAdmin
                {
                    Counterparty = counterpartyInfo,
                    AgentEmail = email,
                    AgentName = agent
                };

                return await _mailSender.Send(_notificationOptions.MasterAgentMailTemplateId, _notificationOptions.AdministratorsEmails, messageData);
            }


            Result<CounterpartyInfo> LogSuccess((CounterpartyInfo, Agent) registrationData)
            {
                var (counterparty, agent) = registrationData;
                _logger.LogAgentRegistrationSuccess($"Agent {agent.Email} with counterparty '{counterparty.Name}' successfully registered");
                return Result.Success(counterparty);
            }


            void LogFailure(string error)
            {
                _logger.LogAgentRegistrationFailed(error);
            }
        }


        private Task AddAgentAgencyRelation(Agent agent, AgentAgencyRelationTypes relationType, InAgencyPermissions permissions, int agencyId)
        {
            _context.AgentAgencyRelations.Add(new AgentAgencyRelation
            {
                AgentId = agent.Id,
                Type = relationType,
                InAgencyPermissions = permissions,
                AgencyId = agencyId,
                IsActive = true
            });

            return _context.SaveChangesAsync();
        }


        private readonly ICounterpartyService _counterpartyService;
        private readonly EdoContext _context;
        private readonly IAgentService _agentService;
        private readonly ILogger<AgentRegistrationService> _logger;
        private readonly MailSenderWithCompanyInfo _mailSender;
        private readonly AgentRegistrationNotificationOptions _notificationOptions;
    }
}