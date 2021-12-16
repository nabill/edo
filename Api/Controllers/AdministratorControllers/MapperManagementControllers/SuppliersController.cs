using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.AdministratorServices.Mapper.AccommodationManagementServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers.MapperManagementControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/mapper/management/suppliers")]
    [Produces("application/json")]
    public class SuppliersController : BaseController
    {
        public SuppliersController(IMapperManagementClient mapperManagementClient)
        {
            _mapperManagementClient = mapperManagementClient;
        }
        
        
        /// <summary>
        /// Returns suppliers list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(Dictionary<int, string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken) 
            => OkOrBadRequest(await _mapperManagementClient.GetSuppliers(cancellationToken));
        
        
        private readonly IMapperManagementClient _mapperManagementClient;
    }
}