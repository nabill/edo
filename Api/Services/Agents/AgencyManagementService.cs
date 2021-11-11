using CSharpFunctionalExtensions;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgencyManagementService : IAgencyManagementService
    {
        public AgencyManagementService(EdoContext context, IAgentService agentService, INotificationService notificationService, 
            IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _agentService = agentService;
            _notificationService = notificationService;
            _dateTimeProvider = dateTimeProvider;
        }


        public Task<Result> DeactivateChildAgency(int agencyId, AgentContext agent)
            => GetAgency(agencyId, agent)
                .Bind(agency => ChangeActivityStatus(agency, ActivityStatus.NotActive));


        public Task<Result> ActivateChildAgency(int agencyId, AgentContext agent)
            => GetAgency(agencyId, agent)
                .Bind(agency => ChangeActivityStatus(agency, ActivityStatus.Active));

        
        private async Task<Result<Agency>> GetAgency(int agencyId, AgentContext agent)
        {
            var agency = await _context.Agencies.SingleOrDefaultAsync(ag => ag.Id == agencyId && ag.ParentId == agent.AgencyId);
            return agency ?? Result.Failure<Agency>("Could not find agency with specified id");
        }
        

        private Task<Result> ChangeActivityStatus(Agency agency, ActivityStatus status)
        {
            var isActive = ToBoolean(status);
            if (isActive == agency.IsActive)
                return Task.FromResult(Result.Success());

            return ChangeAgencyActivityStatus()
                .Tap(ChangeAgencyAccountsActivityStatus)
                .Bind(SendNotificationToMaster);


            async Task<Result> ChangeAgencyActivityStatus()
            {
                agency.IsActive = isActive;
                agency.Modified = _dateTimeProvider.UtcNow();

                _context.Update(agency);
                await _context.SaveChangesAsync();
                return Result.Success();
            }


            async Task ChangeAgencyAccountsActivityStatus()
            {
                var agencyAccounts = await _context.AgencyAccounts
                    .Where(ac => ac.AgencyId == agency.Id)
                    .ToListAsync();

                foreach (var account in agencyAccounts)
                    account.IsActive = isActive;

                _context.UpdateRange(agencyAccounts);
                await _context.SaveChangesAsync();
            }


            async Task<Result> SendNotificationToMaster()
            {
                var (_, isFailure, master, error) = await _agentService.GetMasterAgent(agency.Id);
                if (isFailure)
                    return Result.Failure(error);

                var messageData = new AgencyIsActiveStatusChangedData
                {
                    AgentName = $"{master.FirstName} {master.LastName}",
                    AgencyName = agency.Name,
                    Status = EnumFormatters.FromDescription<ActivityStatus>(status)
                };

                return await _notificationService.Send(agent: new SlimAgentContext(master.Id, agency.Id),
                    messageData: messageData,
                    notificationType: NotificationTypes.AgencyActivityChanged,
                    email: master.Email);
            }
        }


        private static bool ToBoolean(ActivityStatus status)
            => status switch
            {
                ActivityStatus.Active => true,
                _ => false
            };


        private readonly EdoContext _context;
        private readonly IAgentService _agentService;
        private readonly INotificationService _notificationService;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}