using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.AgencyVerificationStatesFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/agency-accounts")]
    [Produces("application/json")]
    public class AgencyAccountsController : BaseController
    {
        public AgencyAccountsController(IAccountPaymentService accountPaymentService)
        {
            _accountPaymentService = accountPaymentService;
        }


        /// <summary>
        ///     Transfers money from an agency account to a child agency account
        /// </summary>
        /// <param name="payerAccountId">Id of the payer agency account</param>
        /// <param name="recipientAccountId">Id of the recepient agency account</param>
        /// <param name="amount">Amount of money to transfer</param>
        [HttpPost("{payerAccountId}/transfer/{recipientAccountId}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [InAgencyPermissions(InAgencyPermissions.AgencyToChildTransfer)]
        public async Task<IActionResult> TransferToChildAgency(int payerAccountId, int recipientAccountId, [FromBody] MoneyAmount amount) 
            => NoContentOrBadRequest(await _accountPaymentService.TransferToChildAgency(payerAccountId, recipientAccountId, amount));


        /// <summary>
        ///     Gets agency accounts list
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<AgencyAccountInfo>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [InAgencyPermissions(InAgencyPermissions.ObserveBalance)]
        public async Task<IActionResult> GetAccounts() 
            => Ok(await _accountPaymentService.GetAgencyAccounts());


        /// <summary>
        ///   Returns account balance for currency
        /// </summary>
        /// <returns>Account balance</returns>
        [HttpGet("currencies/{currency}/balance")]
        [ProducesResponseType(typeof(AccountBalanceInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        [InAgencyPermissions(InAgencyPermissions.ObserveBalance)]
        public async Task<IActionResult> GetAccountBalance(Currencies currency) 
            => OkOrBadRequest(await _accountPaymentService.GetAccountBalance(currency));


        private readonly IAccountPaymentService _accountPaymentService;
    }
}