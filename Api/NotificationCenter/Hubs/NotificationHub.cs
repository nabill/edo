using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.NotificationCenter.Hubs
{
    [Authorize]
    public class NotificationHub : Hub<INotificationClient>
    {
        public NotificationHub(EdoContext context, IAgentContextService agentContextService)
        {
            _context = context;
            _agentContextService = agentContextService;
        }


        /*public static async Task SendPrivateMessage(IHubContext<NotificationHub, INotificationClient> hub, ReceiverTypes receiver, int userId, int messageId, string message)
        {
            await hub.Clients
                .Group(userId.ToString())
                //.User(BuildUserId(receiver, userId))
                .ReceiveMessage(messageId, message);
        }*/


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


        private static string BuildGroupName(int agencyId, int agentId)
            => $"{agencyId}-{agentId}";


        private static string BuildUserId(ReceiverTypes receiver, int userId)
        {
            return receiver switch
            {
                ReceiverTypes.AgentApp => $"agent-{userId}",
                ReceiverTypes.AdminPanel => $"admin-{userId}",
                _ => $"unknown-{userId}",
            };
        }


        private readonly EdoContext _context;
        private readonly IAgentContextService _agentContextService;
    }
}