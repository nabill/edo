using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.Notifications.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.NotificationCenter.Hubs
{
    [Authorize]
    public class AgentNotificationHub : Hub<INotificationClient>
    {
        public AgentNotificationHub(EdoContext context)
        {
            _context = context;
        }


        public override async Task OnConnectedAsync()
        {
            var identityId = Context.User?.FindFirstValue("sub");
            if (string.IsNullOrEmpty(identityId))
                return;

            var agentId = await _context.Agents
                .Where(a => a.IdentityHash == HashGenerator.ComputeSha256(identityId))
                .Select(a => a.Id)
                .SingleOrDefaultAsync();
            if (agentId == default)
                return;

            // TODO: In the future, we will take this information from the token.
            var agencyId = await _context.AgentAgencyRelations
                .Where(aar => aar.AgentId == agentId)
                .Select(aar => aar.AgencyId)
                .SingleOrDefaultAsync();
            if (agencyId == default)
                return;

            await Groups.AddToGroupAsync(Context.ConnectionId, BuildGroupName(agencyId, agentId));
            await base.OnConnectedAsync();
        }

        
        public async Task SendFeedback(NotificationFeedback feedback)
        {
            var notification = feedback.SendingStatus switch
            {
                SendingStatuses.Received
                    => await _context.Notifications
                        .SingleOrDefaultAsync(n => n.Id == feedback.MessageId && (n.SendingStatus == SendingStatuses.Sent)),

                SendingStatuses.Read
                    => await _context.Notifications
                        .SingleOrDefaultAsync(n => n.Id == feedback.MessageId && (n.SendingStatus == SendingStatuses.Sent || n.SendingStatus == SendingStatuses.Received)),

                _ => null
            };
            if (notification is null)
                return;

            notification.SendingStatus = feedback.SendingStatus;
            switch (feedback.SendingStatus)
            {
                case SendingStatuses.Received:
                    notification.Received = feedback.StatusChangeTime;
                    break;
                case SendingStatuses.Read:
                    notification.Read = feedback.StatusChangeTime;
                    break;
            }
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }


        private static string BuildGroupName(int agencyId, int agentId)
            => $"{agencyId}-{agentId}";


        private readonly EdoContext _context;
    }
}