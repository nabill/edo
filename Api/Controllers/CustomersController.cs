using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Services.Customers;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/customers")]
    [Produces("application/json")]
    public class CustomersController : ControllerBase
    {
        public CustomersController(ICustomerRegistrationService customerRegistrationService, ICustomerContext customerContext,
            ICustomerInvitationService customerInvitationService, ITokenInfoAccessor tokenInfoAccessor, DiscoveryClient discoveryClient)
        {
            _customerRegistrationService = customerRegistrationService;
            _customerContext = customerContext;
            _customerInvitationService = customerInvitationService;
            _tokenInfoAccessor = tokenInfoAccessor;
            _discoveryClient = discoveryClient;
        }

        /// <summary>
        ///     Registers master customer with related company
        /// </summary>
        /// <param name="request">Master customer registration request.</param>
        /// <returns></returns>
        [HttpPost("register/master")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RegisterCustomerWithCompany([FromBody] RegisterCustomerWithCompanyRequest request)
        {
            var externalIdentity = _tokenInfoAccessor.GetIdentity();
            if (string.IsNullOrWhiteSpace(externalIdentity))
                return BadRequest(ProblemDetailsBuilder.Build("User sub claim is required"));
            
            var email = await GetUserEmail();
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(ProblemDetailsBuilder.Build("E-mail claim is required"));

            var registerResult = await _customerRegistrationService.RegisterWithCompany(request.Customer, request.Company, 
                    externalIdentity, email);
            
            if (registerResult.IsFailure)
                return BadRequest(ProblemDetailsBuilder.Build(registerResult.Error));

            return NoContent();
        }
        
        /// <summary>
        ///     Registers regular customer.
        /// </summary>
        /// <param name="request">Regular customer registration request.</param>
        /// <returns></returns>
        [HttpPost("register")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RegisterInvitedCustomer([FromBody] RegisterInvitedCustomerRequest request)
        {
            var identity = _tokenInfoAccessor.GetIdentity();
            if (string.IsNullOrWhiteSpace(identity))
                return BadRequest(ProblemDetailsBuilder.Build("User sub claim is required"));
            
            var email = await GetUserEmail();
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(ProblemDetailsBuilder.Build("E-mail claim is required"));
            
            var (_, isFailure, error) = await _customerRegistrationService
                .RegisterInvited(request.RegistrationInfo, request.InvitationCode, identity, email);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }

        /// <summary>
        ///     Invite regular customer.
        /// </summary>
        /// <param name="request">Regular customer registration request.</param>
        /// <returns></returns>
        [HttpPost("invitations")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> InviteCustomer([FromBody] CustomerInvitationInfo request)
        {
            var (_, isFailure, error) = await _customerInvitationService.SendInvitation(request);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }

        /// <summary>
        ///     Get invitation data.
        /// </summary>
        /// <param name="code">Invitation code.</param>
        /// <returns>Invitation data, including prefilled registration information.</returns>
        [HttpGet("invitations/{code}")]
        [ProducesResponseType(typeof(CustomerInvitationInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetInvitationData(string code)
        {
            var (_, isFailure, invitationInfo, error) = await _customerInvitationService
                .GetPendingInvitation(code);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(invitationInfo);
        }

        /// <summary>
        ///     Get current customer.
        /// </summary>
        /// <returns>Current customer information.</returns>
        [HttpGet("")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetCurrentCustomer()
        {
            var (_, isFailure, customer, error) = await _customerContext.GetCustomer();
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(new CustomerInfo(customer.Email,
                customer.LastName,
                customer.FirstName,
                customer.Title,
                customer.Position));
        }
        
        private async Task<string> GetUserEmail()
        {
            var doc = await _discoveryClient.GetAsync();
            var token = await _tokenInfoAccessor.GetAccessToken();
            using (var userInfoClient = new UserInfoClient(doc.UserInfoEndpoint))
            {
                return (await userInfoClient.GetAsync(token))
                    .Claims
                    .SingleOrDefault(c => c.Type == "email")
                    ?.Value;
            }
        }
        
        private readonly ICustomerContext _customerContext;
        private readonly ICustomerInvitationService _customerInvitationService;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
        private readonly DiscoveryClient _discoveryClient;
        private readonly ICustomerRegistrationService _customerRegistrationService;
    }
}