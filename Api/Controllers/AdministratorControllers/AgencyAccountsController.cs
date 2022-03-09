using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums.Administrators;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Money.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin")]
    [Produces("application/json")]
    public class AgencyAccountsController : BaseController
    {
        public AgencyAccountsController(IAdministratorContext administratorContext, IAgencyAccountService agencyAccountService,
            IBalanceNotificationsManagementService balanceNotificationsManagementService)
        {
            _administratorContext = administratorContext;
            _agencyAccountService = agencyAccountService;
            _balanceNotificationsManagementService = balanceNotificationsManagementService;
        }


        /// <summary>
        /// Gets agency accounts list
        /// </summary>
        /// <param name="agencyId">Agency Id</param>
        [HttpGet("agencies/{agencyId}/accounts")]
        [ProducesResponseType(typeof(List<FullAgencyAccountInfo>), StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.AgencyBalanceObservation)]
        public async Task<IActionResult> GetAgencyAccounts([FromRoute] int agencyId) 
            => Ok(await _agencyAccountService.Get(agencyId));
        
        
        /// <summary>
        /// Gets balance for a agency account
        /// </summary>
        /// <param name="agencyId">Agency Id</param>
        /// <param name="currency">Currency</param>
        [HttpGet("agencies/{agencyId}/accounts/{currency}/balance")]
        [ProducesResponseType(typeof(List<FullAgencyAccountInfo>), StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.AgencyBalanceObservation)]
        public async Task<IActionResult> GetAgencyBalance(int agencyId, Currencies currency)
            => Ok(await _agencyAccountService.Get(agencyId, currency));


        /// <summary>
        /// Gets the operation history for the agency account
        /// </summary>
        /// <param name="agencyId">Agency Id</param>
        /// <param name="accountId">Account Id</param>
        [HttpGet("agencies/{agencyId}/accounts/{accountId}/history")]
        [ProducesResponseType(typeof(List<AccountBalanceAuditLogEntry>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgencyBalanceObservation)]
        public async Task<IActionResult> GetAgencyAccountHistory([FromRoute] int agencyId, [FromRoute] int accountId)
            => OkOrBadRequest(await _agencyAccountService.GetAccountHistory(agencyId, accountId));


        /// <summary>
        /// Activates specified agency account.
        /// </summary>
        /// <param name="agencyId">Agency Id.</param>
        /// <param name="agencyAccountId">Agency account Id.</param>
        /// <param name="activityStatusChangeRequest">Request data for activation.</param>
        [HttpPost("agencies/{agencyId}/accounts/{agencyAccountId}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgencyBalanceReplenishAndSubtract)]
        public async Task<IActionResult> ActivateAgencyAccount([FromRoute] int agencyId, [FromRoute] int agencyAccountId,
            [FromBody] ActivityStatusChangeRequest activityStatusChangeRequest)
            => NoContentOrBadRequest(await _agencyAccountService.Activate(agencyId, agencyAccountId, activityStatusChangeRequest.Reason));


        /// <summary>
        /// Deactivates specified agency account.
        /// </summary>
        /// <param name="agencyId">Agency Id.</param>
        /// <param name="agencyAccountId">Agency account Id.</param>
        /// <param name="activityStatusChangeRequest">Request data for deactivation.</param>
        [HttpPost("agencies/{agencyId}/accounts/{agencyAccountId}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgencyBalanceReplenishAndSubtract)]
        public async Task<IActionResult> DeactivateAgencyAccount([FromRoute] int agencyId, [FromRoute] int agencyAccountId,
            [FromBody] ActivityStatusChangeRequest activityStatusChangeRequest)
            => NoContentOrBadRequest(await _agencyAccountService.Deactivate(agencyId, agencyAccountId, activityStatusChangeRequest.Reason));

        
        /// <summary>
        /// Append money to the agency account
        /// </summary>
        /// <param name="agencyAccountId">Id of the agency account</param>
        /// <param name="paymentData">Details about the payment</param>
        [HttpPost("agency-accounts/{agencyAccountId}/replenish")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgencyBalanceReplenishAndSubtract)]
        public async Task<IActionResult> ReplenishAgencyAccount(int agencyAccountId, [FromBody] PaymentData paymentData)
        {
            var (_, _, administrator, _) = await _administratorContext.GetCurrent();
            var (isSuccess, _, error) = await _agencyAccountService.AddMoney(agencyAccountId, paymentData,
                administrator.ToApiCaller());

            return isSuccess
                ? NoContent()
                : (IActionResult) BadRequest(ProblemDetailsBuilder.Build(error));
        }
        

        /// <summary>
        /// Manually Adds money to the agency account
        /// </summary>
        /// <param name="agencyAccountId">Id of the agency account</param>
        /// <param name="paymentData">Details about the payment</param>
        [HttpPost("agency-accounts/{agencyAccountId}/increase-manually")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BalanceManualCorrection)]
        public async Task<IActionResult> IncreaseMoneyManuallyToAgencyAccount(int agencyAccountId, [FromBody] PaymentData paymentData)
        {
            var (_, _, administrator, _) = await _administratorContext.GetCurrent();
            var (isSuccess, _, error) = await _agencyAccountService.IncreaseManually(agencyAccountId, paymentData,
                administrator.ToApiCaller());

            return isSuccess
                ? NoContent()
                : (IActionResult) BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        /// Manually Subtracts money from the agency account
        /// </summary>
        /// <param name="agencyAccountId">Id of the agency account</param>
        /// <param name="paymentData">Details about the payment</param>
        [HttpPost("agency-accounts/{agencyAccountId}/decrease-manually")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BalanceManualCorrection)]
        public async Task<IActionResult> DecreaseMoneyManuallyFromAgencyAccount(int agencyAccountId, [FromBody] PaymentData paymentData)
        {
            var (_, _, administrator, _) = await _administratorContext.GetCurrent();
            var (isSuccess, _, error) = await _agencyAccountService.DecreaseManually(agencyAccountId, paymentData,
                administrator.ToApiCaller());

            return isSuccess
                ? NoContent()
                : (IActionResult)BadRequest(ProblemDetailsBuilder.Build(error));
        }
        
        
        /// <summary>
        /// Subtracts money from the agency account
        /// </summary>
        /// <param name="agencyAccountId">Id of the agency account</param>
        /// <param name="paymentData">Details about the payment</param>
        [HttpPost("agency-accounts/{agencyAccountId}/subtract")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgencyBalanceReplenishAndSubtract)]
        public async Task<IActionResult> SubtractAgencyAccount(int agencyAccountId, [FromBody] PaymentData paymentData)
        {
            var (_, _, administrator, _) = await _administratorContext.GetCurrent();
            var (isSuccess, _, error) = await _agencyAccountService.Subtract(agencyAccountId, paymentData,
                administrator.ToApiCaller());

            return isSuccess
                ? NoContent()
                : (IActionResult) BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        /// Gets thresholds for balance notifications
        /// </summary>
        /// <param name="agencyAccountId">Id of the agency account</param>
        [HttpGet("agency-accounts/{agencyAccountId}/balance-notification-settings")]
        [ProducesResponseType(typeof(BalanceNotificationSettingInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.FinanceReportGeneration)]
        public async Task<IActionResult> GetBalanceNotificationSettings(int agencyAccountId)
            => OkOrBadRequest(await _balanceNotificationsManagementService.Get(agencyAccountId));


        /// <summary>
        /// Sets thresholds for balance notifications
        /// </summary>
        /// <param name="agencyAccountId">Id of the agency account</param>
        [HttpPut("agency-accounts/{agencyAccountId}/balance-notification-settings")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.FinanceReportGeneration)]
        public async Task<IActionResult> SetBalanceNotificationSettings(int agencyAccountId, [FromBody] BalanceNotificationSettingInfo info)
        {
            var (isSuccess, _, error) = await _balanceNotificationsManagementService.Set(agencyAccountId, info.Thresholds);
            
            return isSuccess
                ? NoContent()
                : (IActionResult)BadRequest(ProblemDetailsBuilder.Build(error));
        }


        private readonly IAgencyAccountService _agencyAccountService;
        private readonly IBalanceNotificationsManagementService _balanceNotificationsManagementService;
        private readonly IAdministratorContext _administratorContext;
    }
}