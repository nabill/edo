using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Infrastructure.ModelExtensions;
using Api.Models.Mailing;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Locations;
using HappyTravel.Edo.Data.Management;
using HappyTravel.Edo.Data.Markup;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Markups.Notifications
{
    public class AdminMarkupPolicyNotifications : IAdminMarkupPolicyNotifications
    {
        public AdminMarkupPolicyNotifications(INotificationService notificationService, EdoContext context)
        {
            _notificationService = notificationService;
            _context = context;
        }


        public Task NotifyMarkupAddedOrModified(MarkupPolicy policy, MarkupChangedData changedData)
            => (policy.SubjectScopeType, policy.DestinationScopeType) switch
            {
                (SubjectMarkupScopeTypes.Global, DestinationMarkupScopeTypes.Global) => NotifyGlobalMarkup(policy, changedData),
                (SubjectMarkupScopeTypes.Global, DestinationMarkupScopeTypes.NotSpecified) => NotifyGlobalMarkup(policy, changedData),
                (SubjectMarkupScopeTypes.Global, DestinationMarkupScopeTypes.Market) => NotifyGlobalMarketMarkup(policy, changedData),
                (SubjectMarkupScopeTypes.NotSpecified, DestinationMarkupScopeTypes.Market) => NotifyGlobalMarketMarkup(policy, changedData),
                (SubjectMarkupScopeTypes.Agency, DestinationMarkupScopeTypes.Market) => NotifyAgencyDestinationMarketMarkup(policy, changedData),
                _ => Task.CompletedTask
            };


        private async Task NotifyGlobalMarkup(MarkupPolicy policy, MarkupChangedData changedData)
        {
            var messageData = changedData.FulfillChangedData(policy);

            await _notificationService.Send(messageData, NotificationTypes.MarkupSetUpOrChanged);
        }


        private async Task NotifyGlobalMarketMarkup(MarkupPolicy policy, MarkupChangedData changedData)
        {
            var messageData = changedData.FulfillChangedData(policy);

            var marketName = await _context.Markets
                .Where(m => m.Id.ToString() == policy.DestinationScopeId)
                .Select(m => m.Names.GetValueOrDefault(LocalizationHelper.DefaultLanguageCode))
                .SingleAsync();

            messageData.DestinationScopeName = marketName;

            await _notificationService.Send(messageData, NotificationTypes.MarkupSetUpOrChanged);
        }


        private async Task NotifyAgencyDestinationMarketMarkup(MarkupPolicy policy, MarkupChangedData changedData)
        {
            var messageData = changedData.FulfillChangedData(policy);

            var (targetAdmin, targetAgency, targetMarket) = 
                await (from agency in _context.Agencies.Where(a => a.Id.ToString() == messageData.LocationScopeId)
                    join admin in _context.Administrators on agency.AccountManagerId equals admin.Id into admn
                    from admin in admn.DefaultIfEmpty()
                    from market in _context.Markets.Where(m => m.Id.ToString() == policy.DestinationScopeId)
                    select Tuple.Create<Administrator, Agency, Market>(admin, agency, market))
                .SingleAsync();

            messageData.LocationScopeName = targetAgency?.Name;
            messageData.DestinationScopeName = targetMarket?.Names.GetValueOrDefault(LocalizationHelper.DefaultLanguageCode);

            if (targetAdmin is not null)
                await _notificationService.Send(new SlimAdminContext(targetAdmin.Id), messageData,
                    NotificationTypes.MarkupSetUpOrChanged, targetAdmin.Email);

            await _notificationService.Send(messageData, NotificationTypes.MarkupSetUpOrChanged);
        }


        private readonly INotificationService _notificationService;
        private readonly EdoContext _context;
    }
}