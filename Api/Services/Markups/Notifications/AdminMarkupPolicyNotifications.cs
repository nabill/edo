using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Edo.Data;
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


        public Task NotifyMarkupAddedOrModified(MarkupPolicy newPolicy, MarkupPolicy? oldPolicy)
            => (newPolicy.SubjectScopeType, newPolicy.DestinationScopeType) switch
            {
                (SubjectMarkupScopeTypes.Global, DestinationMarkupScopeTypes.Global) => NotifyGlobalMarkup(newPolicy, oldPolicy),
                (SubjectMarkupScopeTypes.Global, DestinationMarkupScopeTypes.NotSpecified) => NotifyGlobalMarkup(newPolicy, oldPolicy),
                (SubjectMarkupScopeTypes.Global, DestinationMarkupScopeTypes.Market) => NotifyGlobalMarkup(newPolicy, oldPolicy),
                (SubjectMarkupScopeTypes.NotSpecified, DestinationMarkupScopeTypes.Market) => NotifyGlobalMarkup(newPolicy, oldPolicy),
                (SubjectMarkupScopeTypes.Agency, DestinationMarkupScopeTypes.Market) => NotifyAgencyDestinationMarkup(newPolicy, oldPolicy),
                _ => Task.CompletedTask
            };


        private async Task NotifyGlobalMarkup(MarkupPolicy newPolicy, MarkupPolicy? oldPolicy)
        {
            var messageData = new MarkupChangedData()
            {
                PercentChanged = oldPolicy is null ?
                    $"0% => {newPolicy.Value}%" :
                    $"{oldPolicy.Value}% => {newPolicy.Value}%",
                Modified = newPolicy.Modified
            };

            await _notificationService.Send(messageData, NotificationTypes.MarkupSetUpOrChanged);
        }


        private async Task NotifyAgencyDestinationMarkup(MarkupPolicy newPolicy, MarkupPolicy? oldPolicy)
        {
            var admin = await _context.Administrators
                .Join(_context.Agencies.Where(agency => agency.Id.ToString() == newPolicy.SubjectScopeId),
                    admin => admin.Id,
                    agency => agency.AccountManagerId,
                    (admin, agency) => admin)
                .SingleOrDefaultAsync();

            var messageData = new MarkupChangedData()
            {
                PercentChanged = oldPolicy is null ?
                    $"0% => {newPolicy.Value}%" :
                    $"{oldPolicy.Value}% => {newPolicy.Value}%",
                Modified = newPolicy.Modified,
                AgencyId = newPolicy.SubjectScopeId
            };

            if (admin is not null)
                await _notificationService.Send(new SlimAdminContext(admin.Id), messageData, NotificationTypes.MarkupSetUpOrChanged, admin.Email);

            await _notificationService.Send(messageData, NotificationTypes.MarkupSetUpOrChanged);
        }


        private readonly INotificationService _notificationService;
        private readonly EdoContext _context;
    }
}