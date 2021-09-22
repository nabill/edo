using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.AccommodationManagementServices;
using HappyTravel.Edo.Api.AdministratorServices.Models.Mapper;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Common.Enums.Administrators;
using HappyTravel.MapperContracts.Public.Accommodations.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers.MapperManagementControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/mapper/management/accommodations")]
    [Produces("application/json")]
    public class AccommodationController : BaseController
    {
        public AccommodationController(IMapperManagementClient mapperManagementClient)
        {
            _mapperManagementClient = mapperManagementClient;
        }

        
        /// <summary>
        /// Combines two accommodations by htAccommodationId
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("combine")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)] 
        [AdministratorPermissions(AdministratorPermissions.MapperAccommodationManagement)]
        public async Task<IActionResult> Combine([FromBody] CombineAccommodationsRequest request, CancellationToken cancellationToken = default)
            => NoContentOrBadRequest(await _mapperManagementClient.CombineAccommodations(request.BaseHtAccommodationId, request.CombinedHtAccommodationId, cancellationToken));

        
        /// <summary>
        /// Deactivates accommodations by htId with the reason is invalid mapping 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MapperAccommodationManagement)]
        public async Task<IActionResult> DeactivateBecauseOfInvalidMatching([FromBody] DeactivateAccommodationsRequest request, CancellationToken cancellationToken = default)
            => NoContentOrBadRequest(await _mapperManagementClient.DeactivateAccommodations(request, AccommodationDeactivationReasons.WrongMatching, cancellationToken));

        /// <summary>
        /// Removes a supplier from an accommodation
        /// </summary>
        /// <param name="htAccommodationId"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("{htAccommodationId}/suppliers/remove")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MapperAccommodationManagement)]
        public async Task<IActionResult> RemoveSupplier([FromRoute] string htAccommodationId, [FromBody] RemoveSupplierRequest request, CancellationToken cancellationToken = default)
            => NoContentOrBadRequest(await _mapperManagementClient.RemoveSupplier(htAccommodationId, request, cancellationToken));
        
        
        private readonly IMapperManagementClient _mapperManagementClient;
    }
}