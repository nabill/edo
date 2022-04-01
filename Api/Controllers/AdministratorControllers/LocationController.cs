using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Api.AdministratorServices.Locations;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Controllers;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
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
    public class LocationController : BaseController
    {
        public LocationController(IMarketManagementService marketManagementService)
        {
            _marketManagementService = marketManagementService;
        }


        /// <summary>
        ///     Creates market.
        /// </summary>
        /// <param name="namesRequest">Names request</param>
        /// <returns></returns>
        [HttpPost("markets")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> AddMarket([FromBody] IDictionary<string, string> namesRequest)
        {
            var jsonDoc = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes((object)namesRequest, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

            var (_, isFailure, error) = await _marketManagementService.AddMarket(LanguageCode, jsonDoc);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


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
        /// <param name="namesRequest">Names request</param>
        /// <returns></returns>
        [HttpPut("markets/{marketId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MarkupManagement)]
        public async Task<IActionResult> UpdateMarket(int marketId, [FromBody] IDictionary<string, string> namesRequest)
        {
            var jsonDoc = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes((object)namesRequest, new JsonSerializerOptions(JsonSerializerDefaults.Web)));

            var (_, isFailure, error) = await _marketManagementService.UpdateMarket(LanguageCode, marketId, jsonDoc);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


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
        {
            var (_, isFailure, error) = await _marketManagementService.RemoveMarket(marketId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        private readonly IMarketManagementService _marketManagementService;
    }
}