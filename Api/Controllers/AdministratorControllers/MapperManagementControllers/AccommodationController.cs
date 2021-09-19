using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.AccommodationManagementServices;
using HappyTravel.Edo.Api.AdministratorServices.Models.Mapper;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Common.Enums.Administrators;
using HappyTravel.SuppliersCatalog;
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
        {
            var (_, isFailure, _, error) = await _mapperManagementClient.CombineAccommodations(request.BaseHtAccommodationId, request.CombinedHtAccommodationId, cancellationToken);

            if (isFailure)
                return BadRequest(error);

            return NoContent();
        }

        
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
        public async Task<IActionResult> DeactivateBecauseOfInvalidMatching([FromBody] AccommodationsRequest request, CancellationToken cancellationToken = default)
        {
            var (_, isFailure, _, error) = await _mapperManagementClient.DeactivateAccommodations(request, DeactivationReasons.WrongMatching, cancellationToken);

            if (isFailure)
                return BadRequest(error);

            return NoContent();
        }


        [HttpPost("{htAccommodationId}/suppliers/{supplier}/accommodations/{supplierId}/remove")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MapperAccommodationManagement)]
        public async Task<IActionResult> RemoveSupplier(string htAccommodationId, Suppliers supplier, string supplierId, CancellationToken cancellationToken = default)
        {
            var (_, isFailure, _, error) = await _mapperManagementClient.RemoveSupplier(htAccommodationId, supplier, supplierId, cancellationToken);
            if (isFailure)
                return BadRequest(error);

            return NoContent();
        }
        
        
        private readonly IMapperManagementClient _mapperManagementClient;
    }
}