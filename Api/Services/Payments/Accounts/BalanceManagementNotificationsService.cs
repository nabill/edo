using CSharpFunctionalExtensions;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Money.Models;
using System.Linq;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Payments.Accounts
{
    public class BalanceManagementNotificationsService : IBalanceManagementNotificationsService
    {
        // Do not build any functionality based on this service, because it is probably going to be deleted soon
        public BalanceManagementNotificationsService(INotificationService notificationService,
            IBalanceNotificationsManagementService balanceNotificationsManagementService,
            IAdminAgencyManagementService adminAgencyManagementService)
        {
            _notificationService = notificationService;
            _balanceNotificationsManagementService = balanceNotificationsManagementService;
            _adminAgencyManagementService = adminAgencyManagementService;
        }


        public async Task SendNotificationIfRequired(AgencyAccount account, MoneyAmount chargedAmount)
        {
            var resultingBalance = account.Balance - chargedAmount.Amount;

            var (_, isFailure, setting, _) = await _balanceNotificationsManagementService.Get(account.Id);
            if (isFailure || !setting.Thresholds.Any(t => account.Balance >= t && resultingBalance < t))
                return;

            var (_, isAgencyFailure, agency, _) = await _adminAgencyManagementService.Get(account.AgencyId);
            if (isAgencyFailure)
                return;

            var lowestThreshold = setting.Thresholds
                .Where(t => account.Balance >= t && resultingBalance < t)
                .OrderBy(t => t)
                .First();

            var messageData = new AccountBalanceManagementNotificationData
            {
                AgencyAccountId = account.Id,
                AgencyId = agency.Id ?? 0,
                AgencyName = agency.Name,
                Currency = EnumFormatters.FromDescription(account.Currency),
                Threshold = lowestThreshold,
                NewAmount = MoneyFormatter.ToCurrencyString(resultingBalance, account.Currency)
            };

            await _notificationService.Send(messageData: messageData,
                notificationType: NotificationTypes.AccountBalanceManagementNotification);
        }

        
        private readonly INotificationService _notificationService;
        private readonly IBalanceNotificationsManagementService _balanceNotificationsManagementService;
        private readonly IAdminAgencyManagementService _adminAgencyManagementService;
    }
}
