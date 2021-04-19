using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/agencies/{agencyId:int}/discounts")]
    [Produces("application/json")]
    public class AgencyDiscountsController : BaseController
    {
        public AgencyDiscountsController(IAgencyDiscountManagementService discountManagementService)
        {
            _discountManagementService = discountManagementService;
        }


        /// <summary>
        /// Gets discounts for an agency.
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        /// <returns>List of discounts</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<DiscountInfo>), StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> GetAgencyDiscounts([FromRoute] int agencyId)
        {
            return Ok(await _discountManagementService.Get(agencyId));
        }


        /// <summary>
        /// Creates agency discount
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        /// <param name="discount">Discount settings</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> AddDiscount([FromRoute] int agencyId, [FromBody] DiscountInfo discount)
        {
            var (_, isFailure, error) = await _discountManagementService.Add(agencyId, discount);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Deactivates discount
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        /// <param name="discountId">Id of the discount to deactivate.</param>
        /// <returns></returns>
        [HttpPost("{discountId:int}/deactivate")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> DeactivateDiscount([FromRoute] int agencyId, [FromRoute] int discountId)
        {
            var (_, isFailure, error) = await _discountManagementService.Deactivate(agencyId, discountId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Activates discount
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        /// <param name="discountId">Id of the discount to deactivate.</param>
        /// <returns></returns>
        [HttpPost("{discountId:int}/activate")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> ActivateDiscount([FromRoute] int agencyId, [FromRoute] int discountId)
        {
            var (_, isFailure, error) = await _discountManagementService.Activate(agencyId, discountId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Updates policy settings.
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        /// <param name="discountId">Id of the discount.</param>
        /// <param name="discountInfo">Updated settings.</param>
        /// <returns></returns>
        [HttpPut("{discountId:int}")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> UpdateDiscount(int agencyId, int discountId, [FromBody] DiscountInfo discountInfo)
        {
            var (_, isFailure, error) = await _discountManagementService.Update(agencyId, discountId, discountInfo);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        private readonly IAgencyDiscountManagementService _discountManagementService;
    }
}