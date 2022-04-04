using System.Collections.Generic;
using System.Threading.Tasks;
using Api.AdministratorServices.Locations;
using Api.Models.Locations;
using HappyTravel.Edo.Api.Controllers;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/locations")]
    [Produces("application/json")]
    public class MarketManagementController : BaseController
    {
        public MarketManagementController(IMarketManagementService marketManagementService)
        {
            _marketManagementService = marketManagementService;
        }


        /// <summary>
        ///     Creates market.
        /// </summary>
        /// <param name="marketRequest">Market request</param>
        /// <returns></returns>
        [HttpPost("markets")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> AddMarket([FromBody] MarketRequest marketRequest)
            => NoContentOrBadRequest(await _marketManagementService.AddMarket(LanguageCode, marketRequest));


        /// <summary>
        ///     Gets a list of markup markets.
        /// </summary>
        /// <returns>List of markup markets</returns>
        [HttpGet("markets")]
        [ProducesResponseType(typeof(List<Market>), StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> GetMarkets()
            => Ok(await _marketManagementService.GetMarkets(LanguageCode));


        /// <summary>
        ///     Updates market by id.
        /// </summary>
        /// <param name="marketId">Market's id</param>
        /// <param name="marketRequest">Market request</param>
        /// <returns></returns>
        [HttpPut("markets/{marketId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> ModifyMarket(int marketId, [FromBody] MarketRequest marketRequest)
            => NoContentOrBadRequest(await _marketManagementService.ModifyMarket(LanguageCode, new MarketRequest(marketId, marketRequest)));


        /// <summary>
        ///     Deletes market by id.
        /// </summary>
        /// <param name="marketId">Market's id</param>
        /// <returns></returns>
        [HttpDelete("markets/{marketId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> RemoveMarket([FromRoute] int marketId)
            => NoContentOrBadRequest(await _marketManagementService.RemoveMarket(MarketRequest.CreateEmpty(marketId)));


        private readonly IMarketManagementService _marketManagementService;
    }
}