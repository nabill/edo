using System.Linq;
using System.Threading.Tasks;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public class BalanceManagementNotificationsService : IBalanceManagementNotificationsService
    {
        // Do not build any functionality based on this service, because it is probably going to be deleted soon
        public BalanceManagementNotificationsService(EdoContext context,
            INotificationService notificationService,
            IOptions<BalanceManagementNotificationsOptions> balanceManagementNotificationsOptions)
        {
            _context = context;
            _notificationService = notificationService;
            _options = balanceManagementNotificationsOptions.Value;
        }


        public async Task SendNotificationIfRequired(int agencyAccountId, decimal initialBalance, decimal resultingBalance)
        {
            var setting = await _context.BalanceNotificationSettings
                .SingleOrDefaultAsync(s => s.AgencyAccountId == agencyAccountId &&
                    s.Thresholds.Any(t => initialBalance >= t && resultingBalance < t));

            if (setting is null)
                return;

            var lowestThreshold = setting.Thresholds
                .Where(t => initialBalance >= t && resultingBalance < t)
                .OrderBy(t => t)
                .First();
            
            var agencyAndAccount = await (from ag in _context.Agencies
                    join acc in _context.AgencyAccounts on ag.Id equals acc.AgencyId
                    where acc.Id == agencyAccountId
                    select new {Agency = ag, AgencyAccount = acc})
                .SingleAsync();

            var messageData = new AccountBalanceManagementNotificationData
            {
                AgencyAccountId = agencyAccountId,
                AgencyId = agencyAndAccount.Agency.Id,
                AgencyName = agencyAndAccount.Agency.Name,
                Currency = EnumFormatters.FromDescription(agencyAndAccount.AgencyAccount.Currency),
                Threshold = lowestThreshold,
                NewAmount = MoneyFormatter.ToCurrencyString(resultingBalance, agencyAndAccount.AgencyAccount.Currency)
            };

            await _notificationService.Send(messageData: messageData,
                notificationType: NotificationTypes.AccountBalanceManagementNotification,
                email: _options.AccountsEmail,
                templateId: _options.BalanceManagementNotificationTemplateId);
        }


        private readonly EdoContext _context;
        private readonly INotificationService _notificationService;
        private readonly BalanceManagementNotificationsOptions _options;
    }
}
