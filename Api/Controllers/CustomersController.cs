using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Customers;
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
        public CustomersController(IRegistrationService registrationService, ICustomerService customerService)
        {
            _registrationService = registrationService;
            _customerService = customerService;
        }

        /// <summary>
        ///     Registers master customer with related company
        /// </summary>
        /// <param name="request">Master customer registration request.</param>
        /// <returns></returns>
        [HttpPost("registerMaster")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RegisterMasterCustomer([FromBody] RegisterMasterCustomerRequest request)
        {
            var registerResult = await _registrationService.RegisterMasterCustomer(request.Company, request.MasterCustomer);
            if (registerResult.IsFailure)
                return BadRequest(ProblemDetailsBuilder.Build(registerResult.Error));

            return Ok();
        }
        
        /// <summary>
        ///     Returns customer by it's user token id.
        /// </summary>
        /// <param name="userToken">Token.</param>
        /// <returns></returns>
        [HttpPost("")]
        [ProducesResponseType(typeof(CustomerInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetCustomer([FromQuery][Required] string userToken)
        {
            var customer = await _customerService.Get(userToken);
            if (customer.IsFailure)
                return NotFound(ProblemDetailsBuilder.Build(customer.Error));

            return Ok(customer);
        }
        
        private readonly IRegistrationService _registrationService;
        private readonly ICustomerService _customerService;
    }
}