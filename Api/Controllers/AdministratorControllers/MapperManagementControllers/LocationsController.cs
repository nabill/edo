using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.AdministratorServices.Mapper.AccommodationManagementServices;
using HappyTravel.Edo.Api.AdministratorServices.Models.Mapper;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers.MapperManagementControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/mapper/management/locations")]
    [Produces("application/json")]
    public class LocationsController : BaseController
    {
        public LocationsController(IMapperManagementClient mapperManagementClient)
        {
            _mapperManagementClient = mapperManagementClient;
        }


        /// <summary>
        /// Searches countries by query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("countries/search")]
        [ProducesResponseType(typeof(List<CountryData>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.LocationsManagement)]
        public async Task<IActionResult> SearchCountries([FromQuery] [Required] string query, CancellationToken cancellationToken)
            => OkOrBadRequest(await _mapperManagementClient.SearchCountries(query, LanguageCode, cancellationToken));


        /// <summary>
        /// Searches localities by query
        /// </summary>
        /// <param name="countryId"></param>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("countries/{countryId}/localities/search")]
        [ProducesResponseType(typeof(List<LocalityData>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.LocationsManagement)]
        public async Task<IActionResult> SearchLocalities([FromRoute] int countryId, [FromQuery] [Required] string query, CancellationToken cancellationToken)
            => OkOrBadRequest(await _mapperManagementClient.SearchLocalities(countryId, query, LanguageCode, cancellationToken));


        private readonly IMapperManagementClient _mapperManagementClient;
    }
}