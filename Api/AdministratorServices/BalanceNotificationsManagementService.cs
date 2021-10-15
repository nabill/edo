using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class BalanceNotificationsManagementService : IBalanceNotificationsManagementService
    {
        // Do not build any functionality based on this service, because it is probably going to be deleted soon
        public BalanceNotificationsManagementService(EdoContext context)
        {
            _context = context;
        }


        public async Task<Result<BalanceNotificationSettingInfo>> Get(int agencyAccountId)
        {
            var setting = await _context.BalanceNotificationSettings.SingleOrDefaultAsync(s => s.AgencyAccountId == agencyAccountId);
            return new BalanceNotificationSettingInfo {AgencyAccountId = agencyAccountId, Thresholds = setting?.Thresholds ?? new int[0]};
        }


        public Task<Result> Set(int agencyAccountId, int[] thresholds)
        {
            return CheckAccountExists()
                .Tap(Set);


            async Task<Result> CheckAccountExists()
                => await _context.AgencyAccounts.AnyAsync(a => a.Id == agencyAccountId)
                    ? Result.Success()
                    : Result.Failure("Specified account does not exist");


            async Task Set()
            {
                var setting = await _context.BalanceNotificationSettings.SingleOrDefaultAsync(s => s.AgencyAccountId == agencyAccountId);

                if (setting is null)
                {
                    setting = new BalanceNotificationSetting {AgencyAccountId = agencyAccountId, Thresholds = thresholds};
                    _context.Add(setting);
                    await _context.SaveChangesAsync();
                    return;
                }

                setting.Thresholds = thresholds;
                _context.Update(setting);
                await _context.SaveChangesAsync();
            }
        }


        private readonly EdoContext _context;
    }
}
