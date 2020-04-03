using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Filters.Authorization.CounterpartyStatesFilters;
using HappyTravel.Edo.Api.Filters.Authorization.CustomerExistingFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InCounterpartyPermissionFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Models.Customers;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}")]
    [Produces("application/json")]
    public class CustomersController : ControllerBase
    {
        public CustomersController(ICustomerRegistrationService customerRegistrationService, ICustomerContext customerContext,
            ICustomerInvitationService customerInvitationService,
            ITokenInfoAccessor tokenInfoAccessor,
            ICustomerSettingsManager customerSettingsManager,
            ICustomerPermissionManagementService permissionManagementService,
            IHttpClientFactory httpClientFactory,
            ICustomerService customerService)
        {
            _customerRegistrationService = customerRegistrationService;
            _customerContext = customerContext;
            _customerInvitationService = customerInvitationService;
            _tokenInfoAccessor = tokenInfoAccessor;
            _customerSettingsManager = customerSettingsManager;
            _permissionManagementService = permissionManagementService;
            _httpClientFactory = httpClientFactory;
            _customerService = customerService;
        }


        /// <summary>
        ///     Registers master customer with related counterparty
        /// </summary>
        /// <param name="request">Master customer registration request.</param>
        /// <returns></returns>
        [HttpPost("customers/register/master")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RegisterCustomerWithCounterparty([FromBody] RegisterCustomerWithCounterpartyRequest request)
        {
            var externalIdentity = _tokenInfoAccessor.GetIdentity();
            if (string.IsNullOrWhiteSpace(externalIdentity))
                return BadRequest(ProblemDetailsBuilder.Build("User sub claim is required"));

            var email = await GetUserEmail();
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(ProblemDetailsBuilder.Build("E-mail claim is required"));

            var registerResult = await _customerRegistrationService.RegisterWithCounterparty(request.Customer, request.Counterparty,
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
        [HttpPost("customers/register")]
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
        ///     Invite regular customer by e-mail.
        /// </summary>
        /// <param name="request">Regular customer registration request.</param>
        /// <returns></returns>
        [HttpPost("customers/invitations/send")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.ReadOnly)]
        [InCounterpartyPermissions(InCounterpartyPermissions.CustomerInvitation)]
        public async Task<IActionResult> InviteCustomer([FromBody] CustomerInvitationInfo request)
        {
            var (_, isFailure, error) = await _customerInvitationService.Send(request);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        
        /// <summary>
        ///     Create invitation for regular customer.
        /// </summary>
        /// <param name="request">Regular customer registration request.</param>
        /// <returns>Invitation code.</returns>
        [HttpPost("customers/invitations")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [InCounterpartyPermissions(InCounterpartyPermissions.CustomerInvitation)]
        public async Task<IActionResult> CreateInvitation([FromBody] CustomerInvitationInfo request)
        {
            var (_, isFailure, code, error) = await _customerInvitationService.Create(request);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(code);
        }


        /// <summary>
        ///     Get invitation data.
        /// </summary>
        /// <param name="code">Invitation code.</param>
        /// <returns>Invitation data, including pre-filled registration information.</returns>
        [HttpGet("customers/invitations/{code}")]
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
        ///     Gets current customer.
        /// </summary>
        /// <returns>Current customer information.</returns>
        [HttpGet("customers")]
        [ProducesResponseType(typeof(CustomerDescription), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetCurrentCustomer()
        {
            var (_, isFailure, customerInfo, error) = await _customerContext.GetCustomerInfo();
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(new CustomerDescription(customerInfo.Email,
                customerInfo.LastName,
                customerInfo.FirstName,
                customerInfo.Title,
                customerInfo.Position,
                await _customerContext.GetCustomerCounterparties()));
        }
        

        /// <summary>
        ///     Updates current customer properties.
        /// </summary>
        [HttpPut("customers")]
        [ProducesResponseType(typeof(CustomerEditableInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateCurrentCustomer([FromBody] CustomerEditableInfo newInfo)
        {
            var customerRegistrationInfo = await _customerService.UpdateCurrentCustomer(newInfo);
            return Ok(customerRegistrationInfo);
        }


        /// <summary>
        ///     Gets all customers of a counterparty
        /// </summary>
        [HttpGet("companies/{companyId}/customers")]
        [ProducesResponseType(typeof(List<SlimCustomerInfo>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.ReadOnly)]
        [InCounterpartyPermissions(InCounterpartyPermissions.PermissionManagementInCounterparty)]
        public async Task<IActionResult> GetCustomers(int companyId)
        {
            var (_, isFailure, customers, error) = await _customerService.GetCustomers(companyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(customers);
        }


        /// <summary>
        ///     Gets all customers of a branch
        /// </summary>
        [HttpGet("companies/{companyId}/branches/{branchId}/customers")]
        [ProducesResponseType(typeof(List<SlimCustomerInfo>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.ReadOnly)]
        [InCounterpartyPermissions(InCounterpartyPermissions.PermissionManagementInBranch)]
        public async Task<IActionResult> GetCustomers(int companyId, int branchId)
        {
            var (_, isFailure, customers, error) = await _customerService.GetCustomers(companyId, branchId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(customers);
        }


        /// <summary>
        ///     Gets customer of a specified counterparty
        /// </summary>
        [HttpGet("companies/{companyId}/customers/{customerId}")]
        [ProducesResponseType(typeof(CustomerInfoInBranch), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.ReadOnly)]
        [InCounterpartyPermissions(InCounterpartyPermissions.PermissionManagementInCounterparty)]
        public async Task<IActionResult> GetCustomer(int companyId, int customerId)
        {
            var (_, isFailure, customer, error) = await _customerService.GetCustomer(companyId, default, customerId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(customer);
        }


        /// <summary>
        ///     Gets customer of a specified branch
        /// </summary>
        [HttpGet("companies/{companyId}/branches/{branchId}/customers/{customerId}")]
        [ProducesResponseType(typeof(CustomerInfoInBranch), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.ReadOnly)]
        [InCounterpartyPermissions(InCounterpartyPermissions.PermissionManagementInBranch)]
        public async Task<IActionResult> GetCustomer(int companyId, int branchId, int customerId)
        {
            var (_, isFailure, customer, error) = await _customerService.GetCustomer(companyId, branchId, customerId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(customer);
        }


        /// <summary>
        ///     Updates permissions of a customer of a specified branch
        /// </summary>
        [HttpPut("companies/{companyId}/branches/{branchId}/customers/{customerId}/permissions")]
        [ProducesResponseType(typeof(List<InCounterpartyPermissions>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.ReadOnly)]
        public async Task<IActionResult> UpdatePermissionsInBranch(int companyId, int branchId, int customerId,
            [FromBody] List<InCounterpartyPermissions> newPermissions)
        {
            var (_, isFailure, permissions, error) = await _permissionManagementService
                .SetInCounterpartyPermissions(companyId, branchId, customerId, newPermissions);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(permissions);
        }


        /// <summary>
        ///     Sets user frontend application settings.
        /// </summary>
        /// <param name="settings">Settings in dynamic JSON-format</param>
        /// <returns></returns>
        [RequestSizeLimit(256 * 1024)]
        [HttpPut("customers/settings/application")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [CustomerRequired]
        public async Task<IActionResult> SetApplicationSettings([FromBody] JToken settings)
        {
            var customerInfo = await _customerContext.GetCustomer();
            await _customerSettingsManager.SetAppSettings(customerInfo, settings);
            return NoContent();
        }


        /// <summary>
        ///     Gets user frontend application settings.
        /// </summary>
        /// <returns>Settings in dynamic JSON-format</returns>
        [HttpGet("customers/settings/application")]
        [ProducesResponseType(typeof(JToken), (int) HttpStatusCode.OK)]
        [CustomerRequired]
        public async Task<IActionResult> GetApplicationSettings()
        {
            var customerInfo = await _customerContext.GetCustomer();
            var settings = await _customerSettingsManager.GetAppSettings(customerInfo);
            return Ok(settings);
        }


        /// <summary>
        ///     Sets user preferences.
        /// </summary>
        /// <param name="settings">Settings in JSON-format</param>
        /// <returns></returns>
        [HttpPut("customers/settings/user")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [CustomerRequired]
        public async Task<IActionResult> SetUserSettings([FromBody] CustomerUserSettings settings)
        {
            var customerInfo = await _customerContext.GetCustomer();
            await _customerSettingsManager.SetUserSettings(customerInfo, settings);
            return NoContent();
        }


        /// <summary>
        ///     Gets user preferences.
        /// </summary>
        /// <returns>Settings in JSON-format</returns>
        [HttpGet("customers/settings/user")]
        [ProducesResponseType(typeof(CustomerUserSettings), (int) HttpStatusCode.OK)]
        [CustomerRequired]
        public async Task<IActionResult> GetUserSettings()
        {
            var customerInfo = await _customerContext.GetCustomer();
            var settings = await _customerSettingsManager.GetUserSettings(customerInfo);
            return Ok(settings);
        }


        /// <summary>
        ///     Gets all possible permissions
        /// </summary>
        /// <returns> Array of all permission names </returns>
        [HttpGet("all-permissions-list")]
        [ProducesResponseType(typeof(IEnumerable<InCounterpartyPermissions>), (int)HttpStatusCode.OK)]
        [MinCounterpartyState(CounterpartyStates.ReadOnly)]
        public IActionResult GetAllPermissionsList() => Ok(InCounterpartyPermissions.All.ToList().Where(p => p != InCounterpartyPermissions.All));
        

        private async Task<string> GetUserEmail()
        {
            // TODO: Move this logic to separate service
            using var discoveryClient = _httpClientFactory.CreateClient(HttpClientNames.OpenApiDiscovery);
            using var userInfoClient = _httpClientFactory.CreateClient(HttpClientNames.OpenApiUserInfo);

            var doc = await discoveryClient.GetDiscoveryDocumentAsync();
            var token = await _tokenInfoAccessor.GetAccessToken();

            return (await userInfoClient.GetUserInfoAsync(new UserInfoRequest {Token = token, Address = doc.UserInfoEndpoint }))
                .Claims
                .SingleOrDefault(c => c.Type == "email")
                ?.Value;
        }


        private readonly ICustomerContext _customerContext;
        private readonly ICustomerInvitationService _customerInvitationService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerSettingsManager _customerSettingsManager;
        private readonly ICustomerPermissionManagementService _permissionManagementService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICustomerService _customerService;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
    }
}