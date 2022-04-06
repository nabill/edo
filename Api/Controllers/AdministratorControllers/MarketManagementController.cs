using System.Collections.Generic;
using System.Threading.Tasks;
using Api.AdministratorServices.Locations;
using Api.Models.Locations;
using HappyTravel.Edo.Api.Controllers;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Data.Locations;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/locations/markets")]
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
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> AddMarket([FromBody] MarketRequest marketRequest)
            => NoContentOrBadRequest(await _marketManagementService.Add(LanguageCode, marketRequest));


        /// <summary>
        ///     Gets a list of markup markets.
        /// </summary>
        /// <returns>List of markup markets</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Market>), StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> GetMarkets()
            => Ok(await _marketManagementService.Get());


        /// <summary>
        ///     Updates market by id.
        /// </summary>
        /// <param name="marketId">Market's id</param>
        /// <param name="marketRequest">Market request</param>
        /// <returns></returns>
        [HttpPut("{marketId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> ModifyMarket([FromRoute] int marketId, [FromBody] MarketRequest marketRequest)
            => NoContentOrBadRequest(await _marketManagementService.Update(LanguageCode, new MarketRequest(marketId, marketRequest)));


        /// <summary>
        ///     Deletes market by id.
        /// </summary>
        /// <param name="marketId">Market's id</param>
        /// <returns></returns>
        [HttpDelete("{marketId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> RemoveMarket([FromRoute] int marketId)
            => NoContentOrBadRequest(await _marketManagementService.Remove(marketId));


        /// <summary>
        ///     Adds countries to market.
        /// </summary>
        /// <param name="marketId">Market's id</param>
        /// <param name="countryRequest">Country request</param>
        /// <returns></returns>
        [HttpPut("{marketId:int}/countries")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> AddCountries([FromRoute] int marketId, [FromBody] CountryRequest countryRequest)
            => NoContentOrBadRequest(await _marketManagementService.AddCountries(new CountryRequest(marketId, countryRequest)));


        /// <summary>
        ///     Gets a list of countries by market id.
        /// </summary>
        /// <param name="marketId">Market's id</param>
        /// <returns>List of countries of market</returns>
        [HttpGet("{marketId:int}/countries")]
        [ProducesResponseType(typeof(List<Country>), StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> GetCountries([FromRoute] int marketId)
            => OkOrBadRequest(await _marketManagementService.GetCountries(CountryRequest.CreateEmpty(marketId)));


        private readonly IMarketManagementService _marketManagementService;
    }
}