using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
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
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)] 
        public IActionResult Get()
        {
            var (_, isFailure, suppliers, error) = _supplierOptionsStorage.GetAll();
            
            return isFailure
                    ? BadRequest(ProblemDetailsFactory.CreateProblemDetails(HttpContext, detail: error))
                    : Ok(suppliers.ToDictionary(s => s.Code, s => s.Name));
        }


        private readonly ISupplierOptionsStorage _supplierOptionsStorage;
    }
}