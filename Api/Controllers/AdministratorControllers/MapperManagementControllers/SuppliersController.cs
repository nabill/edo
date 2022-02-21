using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.AdministratorServices.Mapper.AccommodationManagementServices;
using HappyTravel.SupplierOptionsProvider;
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
        public SuppliersController(ISupplierOptionsStorage supplierOptionsStorage)
        {
            _supplierOptionsStorage = supplierOptionsStorage;
        }
        
        
        /// <summary>
        /// Returns suppliers list
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            var suppliers = _supplierOptionsStorage.GetAll()
                .ToDictionary(s => s.Code, s => s.Name);
            
            return Ok(suppliers);
        }


        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
    }
}