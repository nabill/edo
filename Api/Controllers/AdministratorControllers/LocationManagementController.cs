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
    public class LocationManagementController : BaseController
    {
        public LocationManagementController(IMarketManagementService marketManagementService, ICountryManagementService countryManagementService)
        {
            _marketManagementService = marketManagementService;
            _countryManagementService = countryManagementService;
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
            => NoContentOrBadRequest(await _marketManagementService.Add(LanguageCode, marketRequest));


        /// <summary>
        ///     Gets a list of markup markets.
        /// </summary>
        /// <returns>List of markup markets</returns>
        [HttpGet("markets")]
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
        [HttpPut("markets/{marketId:int}")]
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
        [HttpDelete("markets/{marketId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> RemoveMarket([FromRoute] int marketId)
            => NoContentOrBadRequest(await _marketManagementService.Remove(marketId));


        /// <summary>
        ///     Gets all list of countries.
        /// </summary>
        /// <returns>List of all countries</returns>
        [HttpGet("countries")]
        [ProducesResponseType(typeof(List<Country>), StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> GetAllCountries()
            => Ok(await _countryManagementService.Get());


        /// <summary>
        ///     Update the composition of the market countries
        /// </summary>
        /// <param name="marketId">Market's id</param>
        /// <param name="countryRequest">Country request</param>
        /// <returns></returns>
        [HttpPut("markets/{marketId:int}/countries")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> UpdateMarketCountries([FromRoute] int marketId, [FromBody] CountryRequest countryRequest)
            => NoContentOrBadRequest(await _marketManagementService.UpdateMarketCountries(new CountryRequest(marketId, countryRequest)));


        /// <summary>
        ///     Gets a list of countries by market id.
        /// </summary>
        /// <param name="marketId">Market's id</param>
        /// <returns>List of countries of market</returns>
        [HttpGet("markets/{marketId:int}/countries")]
        [ProducesResponseType(typeof(List<CountrySlim>), StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> GetMarketCountries([FromRoute] int marketId)
            => OkOrBadRequest(await _marketManagementService.GetMarketCountries(marketId));


        private readonly IMarketManagementService _marketManagementService;
        private readonly ICountryManagementService _countryManagementService;
    }
}