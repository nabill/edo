using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Companies;
using HappyTravel.Edo.Api.Services.Customers;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/customers")]
    [Produces("application/json")]
    public class CustomersController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;

        public CustomersController(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }
        
        [HttpPost("registerMaster")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> RegisterMasterCustomer([FromBody] RegisterMasterCustomerRequest request)
        {
            var registerResult = await _registrationService.RegisterMasterCustomer(request.Company, request.MasterCustomer);
            if(registerResult.IsFailure)
                return BadRequest(registerResult.Error);
            
            return Ok();
        }
    }
}