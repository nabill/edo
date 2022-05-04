using System.Threading.Tasks;
using Api.Models.Markups.Supplier;
using Api.Services.Markups;
using HappyTravel.Edo.Api.Controllers;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CSharpFunctionalExtensions;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Markups;

namespace Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/supplier-markups")]
    [Produces("application/json")]
    public class SupplierMarkupController : BaseController
    {
        public SupplierMarkupController(ISupplierMarkupPolicyManager policyManager)
        {
            _policyManager = policyManager;
        }


        /// <summary>
        ///     Creates supplier markup policy.
        /// </summary>
        /// <param name="request">Markup request</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> AddPolicy([FromBody] SupplierMarkupRequest request)
        {
            var (_, isFailure, error) = await _policyManager.Add(request);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Gets supplier markup policies
        /// </summary>
        /// <returns>List of supplier markups</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<MarkupInfo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> GetPolicies()
            => Ok(await _policyManager.Get());


        /// <summary>
        ///     Updates supplier markup policy.
        /// </summary>
        /// <param name="policyId">Id of the policy.</param>
        /// <param name="request">Updated request.</param>
        /// <returns></returns>
        [HttpPut("{policyId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> ModifyPolicy(int policyId, [FromBody] SupplierMarkupRequest request)
        {
            var (_, isFailure, error) = await _policyManager.Modify(policyId, request);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Deletes supplier policy.
        /// </summary>
        /// <param name="policyId">Id of the policy to delete.</param>
        /// <returns></returns>
        [HttpDelete("{policyId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> RemovePolicy([FromRoute] int policyId)
        {
            var (_, isFailure, error) = await _policyManager.Remove(policyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        private readonly ISupplierMarkupPolicyManager _policyManager;
    }
}