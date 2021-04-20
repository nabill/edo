using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Services.Payments.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin")]
    [Produces("application/json")]
    public class AgencyAccountsController : BaseController
    {
        public AgencyAccountsController(IAccountPaymentService accountPaymentService)
        {
            _accountPaymentService = accountPaymentService;
        }


        /// <summary>
        ///     Gets agency accounts list
        /// </summary>
        /// <param name="agencyId">Agency Id</param>
        [HttpGet("{agencyId}/agency-accounts")]
        [ProducesResponseType(typeof(List<FullAgencyAccountInfo>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyBalanceObservation)]
        public async Task<IActionResult> GetAgencyAccounts([FromRoute] int agencyId) 
            => Ok(await _accountPaymentService.GetAgencyAccounts(agencyId));


        /// <summary>
        /// Changes an agency account activity state
        /// </summary>
        /// <param name="agencyId">Agency Id</param>
        /// <param name="agencyAccountId">Agency account Id</param>
        /// <param name="agencyAccountRequest">Editable agency account settings</param>
        [HttpPut("{agencyId}/agency-accounts/{agencyAccountId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyBalanceReplenishAndSubtract)]
        public async Task<IActionResult> SetAgencyAccountSettings([FromRoute] int agencyId, [FromRoute] int agencyAccountId, [FromBody] AgencyAccountRequest agencyAccountRequest)
        {
            var (_, isFailure, error) = await _accountPaymentService.SetAgencyAccountSettings(new AgencyAccountSettings(agencyId, agencyAccountId, agencyAccountRequest.IsActive));
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok();
        }


        private readonly IAccountPaymentService _accountPaymentService;
    }
}