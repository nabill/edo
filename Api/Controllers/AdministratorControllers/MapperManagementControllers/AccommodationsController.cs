using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Mapper.AccommodationManagementServices;
using HappyTravel.Edo.Api.AdministratorServices.Models.Mapper;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Common.Enums.Administrators;
using HappyTravel.MapperContracts.Public.Management.Accommodations.ManualCorrections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers.MapperManagementControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/mapper/management/accommodations")]
    [Produces("application/json")]
    public class AccommodationsController : BaseController
    {
        public AccommodationsController(IMapperManagementClient mapperManagementClient, IAccommodationMapperClient accommodationMapperClient)
        {
            _mapperManagementClient = mapperManagementClient;
            _accommodationMapperClient = accommodationMapperClient;
        }


        /// <summary>
        /// Combines two accommodations by htAccommodationId
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("merge")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AccommodationsMerge)]
        public async Task<IActionResult> Merge([FromBody] AccommodationsMergeRequest request, CancellationToken cancellationToken = default)
            => NoContentOrBadRequest(await _mapperManagementClient.MergeAccommodations(request, cancellationToken));


        /// <summary>
        /// Returns mapper accommodation info
        /// </summary>
        /// <param name="htAccommodationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Raw accommodation details</returns>
        [HttpGet("{htAccommodationId}")]
        [ProducesResponseType(typeof(MapperContracts.Public.Accommodations.Accommodation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AccommodationsMerge)]
        public async Task<IActionResult> Get([FromRoute] string htAccommodationId, CancellationToken cancellationToken = default)
            => OkOrBadRequest(await _accommodationMapperClient.GetAccommodation(htAccommodationId, LanguageCode, cancellationToken));


        /// <summary>
        /// Deactivates accommodation
        /// </summary>
        /// <param name="htAccommodationId"></param>
        /// <param name="deactivateAccommodationDescriptionRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("{htAccommodationId}/deactivate-manually")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AccommodationsManagement)]
        public async Task<IActionResult> DeactivateAccommodationManually([FromRoute] string htAccommodationId,
            [FromBody] DeactivateAccommodationDescriptionRequest deactivateAccommodationDescriptionRequest, CancellationToken cancellationToken = default)
            => NoContentOrBadRequest(await _mapperManagementClient.DeactivateAccommodationManually(htAccommodationId, deactivateAccommodationDescriptionRequest.DeactivationReasonDescription, cancellationToken));


        /// <summary>
        /// Activates accommodation 
        /// </summary>
        /// <param name="htAccommodationId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("{htAccommodationId}/activate-manually")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AccommodationsManagement)]
        public async Task<IActionResult> ActivateAccommodationManually([FromRoute] string htAccommodationId, CancellationToken cancellationToken = default)
            => NoContentOrBadRequest(await _mapperManagementClient.ActivateAccommodationManually(htAccommodationId, cancellationToken));


        /// <summary>
        /// Retrieves accommodation detailed data
        /// </summary>
        /// <param name="accommodationHtId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Detailed accommodation</returns>
        [HttpGet("{accommodationHtId}/detailed-data")]
        [ProducesResponseType(typeof(DetailedAccommodation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AccommodationsManagement)]
        public async Task<IActionResult> GetDetailedAccommodationData([FromRoute] string accommodationHtId, CancellationToken cancellationToken)
        {
            var (_, isFailure, accommodationData, problemDetails) = await _mapperManagementClient.GetDetailedAccommodationData(accommodationHtId, LanguageCode,
                cancellationToken);
            
            if (isFailure)
                return BadRequest(problemDetails);
            
            return Ok(MapperContractsConverter.Convert(accommodationData) );
        }


        /// <summary>
        /// Searches accommodations by search criteria
        /// </summary>
        /// <param name="searchRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("search")]
        [ProducesResponseType(typeof(List<SlimAccommodationData>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AccommodationsManagement)]
        public async Task<IActionResult> SearchAccommodations([FromBody] AccommodationSearchRequest searchRequest, CancellationToken cancellationToken)
            => OkOrBadRequest(await _mapperManagementClient.SearchAccommodations(searchRequest, cancellationToken));


        /// <summary>
        /// Retrieves rating types
        /// </summary>
        /// <returns></returns>
        [HttpGet("rating-types")]
        [ProducesResponseType(typeof(Dictionary<int, string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AccommodationsManagement)]
        public async Task<IActionResult> GetRatingTypes(CancellationToken cancellationToken)
            => OkOrBadRequest(await _mapperManagementClient.GetRatingTypes(cancellationToken));


        /// <summary>
        /// Retrieves deactivation reason types
        /// </summary>
        /// <returns></returns>
        [HttpGet("deactivation-reason-types")]
        [ProducesResponseType(typeof(Dictionary<int, string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AccommodationsManagement)]
        public async Task<IActionResult> GetDeactivationReasonTypes(CancellationToken cancellationToken)
            => OkOrBadRequest(await _mapperManagementClient.GetDeactivationReasonTypes(cancellationToken));


        /// <summary>
        /// Adds an accommodation data corrections. Values will be applied after the mapper updater is run
        /// </summary>
        /// <param name="htAccommodationId"></param>
        /// <param name="accommodationManualCorrectionRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("{htAccommodationId}/manual-correction")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AccommodationsManagement)]
        public async Task<IActionResult> AddManualCorrectionData([FromRoute] string htAccommodationId,
            [FromBody] AccommodationManualCorrectionRequest accommodationManualCorrectionRequest, CancellationToken cancellationToken = default)
            => NoContentOrBadRequest(await _mapperManagementClient.AddManualCorrectionData(htAccommodationId, accommodationManualCorrectionRequest, cancellationToken));


        private readonly IMapperManagementClient _mapperManagementClient;
        private readonly IAccommodationMapperClient _accommodationMapperClient;
    }
}