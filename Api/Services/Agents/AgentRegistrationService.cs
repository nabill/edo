using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Models.Locations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgentRegistrationService : IAgentRegistrationService
    {
        public AgentRegistrationService(EdoContext context,
            ICounterpartyService counterpartyService,
            IAgentService agentService,
            IOptions<AgentRegistrationNotificationOptions> notificationOptions,
            INotificationService notificationService,
            MailSenderWithCompanyInfo mailSender,
            ILogger<AgentRegistrationService> logger)
        {
            _context = context;
            _counterpartyService = counterpartyService;
            _agentService = agentService;
            _notificationOptions = notificationOptions.Value;
            _notificationService = notificationService;
            _logger = logger;
        }


        public Task<Result> RegisterWithCounterparty(UserDescriptionInfo agentData, CounterpartyCreateRequest counterpartyData, string externalIdentity,
            string email, LocalityInfo localityInfo)
        {
            return Result.Success()
                .Ensure(IsIdentityPresent, "User should have identity")
                .BindWithTransaction(_context, () => Result.Success()
                    .Bind(CreateCounterparty)
                    .Tap(AddLocalityInfo)
                    .Bind(CreateAgent)
                    .Tap(AddMasterAgentAgencyRelation))
                .Bind(LogSuccess)
                .Bind(SendRegistrationMailToAdmins)
                .OnFailure(LogFailure);


            bool IsIdentityPresent() => !string.IsNullOrWhiteSpace(externalIdentity);


            Task<Result<SlimCounterpartyInfo>> CreateCounterparty() 
                => _counterpartyService.Add(counterpartyData);


            async Task<Result<(SlimCounterpartyInfo, Agent)>> CreateAgent(SlimCounterpartyInfo counterparty)
            {
                var (_, isFailure, agent, error) = await _agentService.Add(agentData, externalIdentity, email);
                return isFailure
                    ? Result.Failure<(SlimCounterpartyInfo, Agent)>(error)
                    : Result.Success((counterparty1: counterparty, agent));
            }


            async Task<SlimCounterpartyInfo> AddLocalityInfo(SlimCounterpartyInfo counterpartyInfo)
            {
                var rootAgency = await _counterpartyService.GetRootAgency(counterpartyInfo.Id);

                rootAgency.CountryCode = localityInfo.CountryIsoCode;
                rootAgency.CountryHtId = localityInfo.CountryHtId;
                rootAgency.City = localityInfo.LocalityName;
                rootAgency.LocalityHtId = localityInfo.LocalityHtId;
                
                _context.Agencies.Add(rootAgency);

                return counterpartyInfo;
            }


            async Task AddMasterAgentAgencyRelation((SlimCounterpartyInfo counterparty, Agent agent) counterpartyAgentInfo)
            {
                var (counterparty, agent) = counterpartyAgentInfo;
                var rootAgency = await _counterpartyService.GetRootAgency(counterparty.Id);
                
                // assign all roles to master agent
                var roleIds = await _context.AgentRoles.Select(x => x.Id).ToArrayAsync();
                
                await AddAgentAgencyRelation(agent,
                    AgentAgencyRelationTypes.Master,
                    rootAgency.Id,
                    roleIds);
            }


            async Task<Result> SendRegistrationMailToAdmins(SlimCounterpartyInfo counterpartyInfo)
            {
                var agent = $"{agentData.Title} {agentData.FirstName} {agentData.LastName}";
                if (!string.IsNullOrWhiteSpace(agentData.Position))
                    agent += $" ({agentData.Position})";

                var agency = await _counterpartyService.GetRootAgency(counterpartyInfo.Id);

                var messageData = new RegistrationDataForAdmin
                {
                    Counterparty = new RegistrationDataForAdmin.CounterpartyRegistrationMailData
                    {
                        Name = counterpartyInfo.Name,
                        CountryCode = agency.CountryCode,
                        City = agency.City,
                        Phone = agency.Phone,
                        PostalCode = agency.PostalCode,
                        Fax = agency.Fax,
                        PreferredCurrency = EnumFormatters.FromDescription(agency.PreferredCurrency),
                        Website = agency.Website
                    },
                    AgentEmail = email,
                    AgentName = agent
                };

                return await _notificationService.Send(messageData: messageData,
                    notificationType: NotificationTypes.MasterAgentSuccessfulRegistration,
                    emails: _notificationOptions.AdministratorsEmails,
                    templateId: _notificationOptions.MasterAgentMailTemplateId);
            }


            Result<SlimCounterpartyInfo> LogSuccess((SlimCounterpartyInfo, Agent) registrationData)
            {
                var (counterparty, agent) = registrationData;
                _logger.LogAgentRegistrationSuccess(agent.Email);
                return Result.Success(counterparty);
            }


            void LogFailure(string error)
            {
                _logger.LogAgentRegistrationFailed(error);
            }
        }


        private Task AddAgentAgencyRelation(Agent agent, AgentAgencyRelationTypes relationType, int agencyId, int[] agentRoleIds)
        {
            _context.AgentAgencyRelations.Add(new AgentAgencyRelation
            {
                AgentId = agent.Id,
                Type = relationType,
                AgencyId = agencyId,
                IsActive = true,
                AgentRoleIds = agentRoleIds
            });

            return _context.SaveChangesAsync();
        }


        private readonly ICounterpartyService _counterpartyService;
        private readonly EdoContext _context;
        private readonly IAgentService _agentService;
        private readonly ILogger<AgentRegistrationService> _logger;
        private readonly INotificationService _notificationService;
        private readonly AgentRegistrationNotificationOptions _notificationOptions;
    }
}