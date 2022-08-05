using System.Net;
using System.Threading.Tasks;
using Api.AdministratorServices;
using HappyTravel.Edo.Api.Controllers;
using HappyTravel.Edo.Api.Filters.Authorization.ServiceAccountFilters;
using HappyTravel.Edo.Api.Models.Bookings;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/internal/agencies")]
    public class InternalAgencyController : BaseController
    {
        public InternalAgencyController(IBalanceNotificationsService balanceNotificationService)
        {
            _balanceNotificationService = balanceNotificationService;
        }


        /// <summary>
        ///     Sends notifications when funds on the agency balance decrease below thresholds
        /// </summary>
        /// <returns>Result message</returns>
        [HttpPost("notifications/credit-limit-run-out-balance/send")]
        [ProducesResponseType(typeof(BatchOperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> NotifyCreditLimitRunOutBalance()
            => OkOrBadRequest(await _balanceNotificationService.NotifyCreditLimitRunOutBalance());


        private readonly IBalanceNotificationsService _balanceNotificationService;
    }
}