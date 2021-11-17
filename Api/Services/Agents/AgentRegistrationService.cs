using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Models.Agencies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgentRegistrationService : IAgentRegistrationService
    {
        public AgentRegistrationService(EdoContext context,
            IAgentService agentService,
            IOptions<AgentRegistrationNotificationOptions> notificationOptions,
            INotificationService notificationService,
            ILogger<AgentRegistrationService> logger,
            IAgencyService agencyService)
        {
            _context = context;
            _agentService = agentService;
            _notificationOptions = notificationOptions.Value;
            _notificationService = notificationService;
            _logger = logger;
            _agencyService = agencyService;
        }


        public Task<Result> RegisterWithAgency(UserDescriptionInfo agentData, RegistrationAgencyInfo registrationAgencyInfo, string externalIdentity,
            string email)
        {
            return Result.Success()
                .Ensure(IsIdentityPresent, "User should have identity")
                .BindWithTransaction(_context, () => Result.Success()
                    .Bind(CreateRootAgency)
                    .Bind(CreateAgent)
                    .Tap(AddMasterAgentAgencyRelation))
                .Bind(LogSuccess)
                .Bind(SendRegistrationMailToAdmins)
                .OnFailure(LogFailure);


            bool IsIdentityPresent() => !string.IsNullOrWhiteSpace(externalIdentity);


            Task<Result<AgencyInfo>> CreateRootAgency() 
                => _agencyService.Create(registrationAgencyInfo, counterpartyId: 0, parentAgencyId: null);


            async Task<Result<(AgencyInfo, Agent)>> CreateAgent(AgencyInfo agency)
            {
                var (_, isFailure, agent, error) = await _agentService.Add(agentData, externalIdentity, email);
                return isFailure
                    ? Result.Failure<(AgencyInfo, Agent)>(error)
                    : Result.Success((agency, agent));
            }


            async Task AddMasterAgentAgencyRelation((AgencyInfo agency, Agent agent) agencyAgentInfo)
            {
                var (agency, agent) = agencyAgentInfo;
                
                // assign all roles to master agent
                var roleIds = await _context.AgentRoles.Select(x => x.Id).ToArrayAsync();
                
                await AddAgentAgencyRelation(agent,
                    AgentAgencyRelationTypes.Master,
                    agency.Id.Value,
                    roleIds);
            }


            async Task<Result> SendRegistrationMailToAdmins(AgencyInfo agency)
            {
                var agent = $"{agentData.Title} {agentData.FirstName} {agentData.LastName}";
                if (!string.IsNullOrWhiteSpace(agentData.Position))
                    agent += $" ({agentData.Position})";

                var messageData = new RegistrationDataForAdmin
                {
                    Agency = new RegistrationDataForAdmin.RootAgencyRegistrationMailData
                    {
                        Name = agency.Name,
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
                    emails: _notificationOptions.AdministratorsEmails);
            }


            Result<AgencyInfo> LogSuccess((AgencyInfo, Agent) registrationData)
            {
                var (agency, agent) = registrationData;
                _logger.LogAgentRegistrationSuccess(agent.Email);
                return Result.Success(agency);
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

        
        private readonly EdoContext _context;
        private readonly IAgentService _agentService;
        private readonly ILogger<AgentRegistrationService> _logger;
        private readonly IAgencyService _agencyService;
        private readonly INotificationService _notificationService;
        private readonly AgentRegistrationNotificationOptions _notificationOptions;
    }
}