using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.FunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Agents
{
    public class AgencyManagementService : IAgencyManagementService
    {
        public AgencyManagementService(EdoContext context, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }


        public Task<Result> DeactivateAgency(int agencyId, AgentContext agent)
            => GetAgency(agencyId, agent)
                .BindWithTransaction(_context, agency => ChangeActivityStatus(agency, ActivityStatus.NotActive));


        public Task<Result> ActivateAgency(int agencyId, AgentContext agent)
            => GetAgency(agencyId, agent)
                .BindWithTransaction(_context, agency => ChangeActivityStatus(agency, ActivityStatus.Active));

        
        private async Task<Result<Agency>> GetAgency(int agencyId, AgentContext agent)
        {
            var agency = await _context.Agencies.FirstOrDefaultAsync(ag => ag.Id == agencyId && ag.ParentId == agent.AgencyId);
            return agency ?? Result.Failure<Agency>("Could not find agency with specified id");
        }
        

        private Task<Result> ChangeActivityStatus(Agency agency, ActivityStatus status)
        {
            var convertedStatus = ConvertToDbStatus(status);
            if (convertedStatus == agency.IsActive)
                return Task.FromResult(Result.Success());

            return ChangeAgencyActivityStatus()
                .Tap(ChangeAgencyAccountsActivityStatus);


            async Task<Result> ChangeAgencyActivityStatus()
            {
                agency.IsActive = convertedStatus;
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
                    account.IsActive = convertedStatus;

                _context.UpdateRange(agencyAccounts);
                await _context.SaveChangesAsync();
            }
        }
        
        
        private static bool ConvertToDbStatus(ActivityStatus status) => status == ActivityStatus.Active;


        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}