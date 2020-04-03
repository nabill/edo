using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Filters.Authorization.CounterpartyStatesFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InCounterpartyPermissionFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Branches;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/companies")]
    [Produces("application/json")]
    public class CounterpartiesController : BaseController
    {
        public CounterpartiesController(ICounterpartyService _counterpartyService)
        {
            _counterpartyService = _counterpartyService;
        }


        /// <summary>
        ///     Sets counterparty fully verified.
        /// </summary>
        /// <param name="companyId">Id of the counterparty to verify.</param>
        /// <param name="request">Verification details.</param>
        /// <returns></returns>
        [HttpPost("{companyId}/verify")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyVerification)]
        public async Task<IActionResult> Verify(int companyId, [FromBody] CounterpartyVerificationRequest request)
        {
            var (isSuccess, _, error) = await _counterpartyService.VerifyAsFullyAccessed(companyId, request.Reason);

            return isSuccess
                ? (IActionResult) NoContent()
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        ///     Sets counterparty read-only verified.
        /// </summary>
        /// <param name="companyId">Id of the counterparty to verify.</param>
        /// <param name="request">Verification details.</param>
        /// <returns></returns>
        [HttpPost("{companyId}/verify/read-only")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.CounterpartyVerification)]
        public async Task<IActionResult> VerifyAsReadOnly(int companyId, [FromBody] CounterpartyVerificationRequest request)
        {
            var (isSuccess, _, error) = await _counterpartyService.VerifyAsReadOnly(companyId, request.Reason);

            return isSuccess
                ? (IActionResult) NoContent()
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        ///     Creates branch for counterparty.
        /// </summary>
        /// <param name="companyId">Counterparty Id.</param>
        /// <param name="branchInfo">Branch information.</param>
        /// <returns></returns>
        [HttpPost("{companyId}/branches")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AddBranch(int companyId, [FromBody] BranchInfo branchInfo)
        {
            var (isSuccess, _, _, error) = await _counterpartyService.AddBranch(companyId, branchInfo);

            return isSuccess
                ? (IActionResult)NoContent()
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        ///     Gets branch.
        /// </summary>
        /// <param name="companyId">Counterparty Id.</param>
        /// <param name="branchId">Branch Id.</param>
        /// <returns></returns>
        [HttpGet("{companyId}/branches/{branchId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetBranch(int companyId, int branchId)
        {
            var (_, isFailure, branch, error) = await _counterpartyService.GetBranch(companyId, branchId);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(branch);
        }


        /// <summary>
        ///     Gets all branches of a counterparty.
        /// </summary>
        /// <param name="companyId">Counterparty Id.</param>
        /// <returns></returns>
        [HttpGet("{companyId}/branches")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetBranches(int companyId)
        {
            var (_, isFailure, branch, error) = await _counterpartyService.GetAllCounterpartyBranches(companyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(branch);
        }


        /// <summary>
        ///     Updates counterparty information.
        /// </summary>
        /// <param name="companyId">Id of the counterparty to verify.</param>
        /// <param name="updatedCounterpartyInfo">New counterparty information.</param>
        /// <returns></returns>
        [HttpPut("{companyId}")]
        [ProducesResponseType(typeof(CounterpartyInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.ReadOnly)]
        [InCounterpartyPermissions(InCounterpartyPermissions.EditCounterpartyInfo)]
        public async Task<IActionResult> UpdateCounterparty(int companyId, [FromBody] CounterpartyInfo updatedCounterpartyInfo)
        {
            var (_, isFailure, savedCompanyInfo, error) = await _counterpartyService.Update(updatedCounterpartyInfo, companyId);
            
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(savedCompanyInfo);
        }

        /// <summary>
        ///     Gets counterparty information.
        /// </summary>
        /// <param name="companyId">Id of the counterparty to verify.</param>
        /// <returns></returns>
        [HttpGet("{companyId}")]
        [ProducesResponseType(typeof(CounterpartyInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetCounterparty(int companyId)
        {
            var (_, isFailure, companyInfo, error) = await _counterpartyService.Get(companyId);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(companyInfo);
        }

        private readonly ICounterpartyService _counterpartyService;
    }
}