using System;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/agencies")]
    [Produces("application/json")]
    public class AgencySettingsController : BaseController
    {

        public AgencySettingsController(IAgentContextService agentContextService,
            IAgencySystemSettingsService agencySystemSettingsService)
        {
            _agentContextService = agentContextService;
            _agencySystemSettingsService = agencySystemSettingsService;
        }


        /// <summary>
        ///     Gets setting which tells what payment methods to show for booking payment.
        /// </summary>
        /// <param name="agencyId">Id of an agency to get settings for</param>
        /// <returns>Displayed payment methods setting</returns>
        [HttpGet("{agencyId}/system-settings/displayed-payment-options")]
        [ProducesResponseType(typeof(DisplayedPaymentOptionsSettings), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetDisplayedPaymentOptions([FromRoute] int agencyId)
        {
            var agent = await _agentContextService.GetAgent();
            return OkOrBadRequest(await _agencySystemSettingsService.GetDisplayedPaymentOptions(agencyId, agent));
        }


        private readonly IAgentContextService _agentContextService;
        private readonly IAgencySystemSettingsService _agencySystemSettingsService;
    }
}
