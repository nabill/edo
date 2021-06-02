using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.NotificationCenter.Models;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Api.Services.Agents;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/notifications")]
    [Produces("application/json")]
    public class NotificationController : BaseController
    {
        public NotificationController(IAgentContextService agentContextService, INotificationService notificationService, INotificationOptionsService notificationOptionsService)
        {
            _agentContextService = agentContextService;
            _notificationService = notificationService;
            _notificationOptionsService = notificationOptionsService;
        }


        /// <summary>
        ///     Gets the notification history of the current agent
        /// </summary>
        /// <param name="skip">Skip</param>
        /// <param name="top">Top</param>
        /// <returns>List of notifications</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<SlimNotification>), (int)HttpStatusCode.OK)]
        [AgentRequired]
        public async Task<IActionResult> GetNotifications([FromQuery] int skip = 0, [FromQuery] int top = 1000)
        {
            var agent = await _agentContextService.GetAgent();

            return Ok(await _notificationService.Get(new SlimAgentContext(agent.AgentId, agent.AgencyId), skip, top));
        }


        /// <summary>
        ///     Gets the notification options of the current agent
        /// </summary>
        /// <returns>List of notification options</returns>
        [HttpGet("options")]
        [ProducesResponseType(typeof(List<SlimNotificationOptions>), (int)HttpStatusCode.OK)]
        [AgentRequired]
        public async Task<IActionResult> GetNotificationOptions()
        {
            var agent = await _agentContextService.GetAgent();

            return Ok(await _notificationOptionsService.Get(new SlimAgentContext(agent.AgentId, agent.AgencyId)));
        }


        private readonly IAgentContextService _agentContextService;
        private readonly INotificationService _notificationService;
        private readonly INotificationOptionsService _notificationOptionsService;
    }
}
