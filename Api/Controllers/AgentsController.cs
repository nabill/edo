using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Filters.Authorization.CounterpartyStatesFilters;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
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
    public class AgentsController : ControllerBase
    {
        public AgentsController(IAgentRegistrationService agentRegistrationService, IAgentContextService agentContextService,
            IAgentContextInternal agentContextInternal,
            IAgentInvitationService agentInvitationService,
            ITokenInfoAccessor tokenInfoAccessor,
            IAgentSettingsManager agentSettingsManager,
            IAgentPermissionManagementService permissionManagementService,
            IHttpClientFactory httpClientFactory,
            IAgentService agentService)
        {
            _agentRegistrationService = agentRegistrationService;
            _agentContextService = agentContextService;
            _agentContextInternal = agentContextInternal;
            _agentInvitationService = agentInvitationService;
            _tokenInfoAccessor = tokenInfoAccessor;
            _agentSettingsManager = agentSettingsManager;
            _permissionManagementService = permissionManagementService;
            _httpClientFactory = httpClientFactory;
            _agentService = agentService;
        }


        /// <summary>
        ///     Registers master agent with related counterparty
        /// </summary>
        /// <param name="request">Master agent registration request.</param>
        /// <returns></returns>
        [HttpPost("agents/register/master")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RegisterAgentWithCounterparty([FromBody] RegisterAgentWithCounterpartyRequest request)
        {
            var externalIdentity = _tokenInfoAccessor.GetIdentity();
            if (string.IsNullOrWhiteSpace(externalIdentity))
                return BadRequest(ProblemDetailsBuilder.Build("User sub claim is required"));

            var email = await GetUserEmail();
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(ProblemDetailsBuilder.Build("E-mail claim is required"));

            var registerResult = await _agentRegistrationService.RegisterWithCounterparty(request.Agent, request.Counterparty,
                externalIdentity, email);

            if (registerResult.IsFailure)
                return BadRequest(ProblemDetailsBuilder.Build(registerResult.Error));

            return NoContent();
        }


        /// <summary>
        ///     Registers regular agent.
        /// </summary>
        /// <param name="request">Regular agent registration request.</param>
        /// <returns></returns>
        [HttpPost("agents/register")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RegisterInvitedAgent([FromBody] RegisterInvitedAgentRequest request)
        {
            var identity = _tokenInfoAccessor.GetIdentity();
            if (string.IsNullOrWhiteSpace(identity))
                return BadRequest(ProblemDetailsBuilder.Build("User sub claim is required"));

            var email = await GetUserEmail();
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(ProblemDetailsBuilder.Build("E-mail claim is required"));

            var (_, isFailure, error) = await _agentRegistrationService
                .RegisterInvited(request.RegistrationInfo, request.InvitationCode, identity, email);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Invite regular agent by e-mail.
        /// </summary>
        /// <param name="request">Regular agent registration request.</param>
        /// <returns></returns>
        [HttpPost("agents/invitations/send")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.ReadOnly)]
        [InAgencyPermissions(InAgencyPermissions.AgentInvitation)]
        public async Task<IActionResult> InviteAgent([FromBody] AgentInvitationInfo request)
        {
            var (_, isFailure, error) = await _agentInvitationService.Send(request);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        
        /// <summary>
        ///     Create invitation for regular agent.
        /// </summary>
        /// <param name="request">Regular agent registration request.</param>
        /// <returns>Invitation code.</returns>
        [HttpPost("agents/invitations")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [InAgencyPermissions(InAgencyPermissions.AgentInvitation)]
        public async Task<IActionResult> CreateInvitation([FromBody] AgentInvitationInfo request)
        {
            var (_, isFailure, code, error) = await _agentInvitationService.Create(request);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(code);
        }


        /// <summary>
        ///     Get invitation data.
        /// </summary>
        /// <param name="code">Invitation code.</param>
        /// <returns>Invitation data, including pre-filled registration information.</returns>
        [HttpGet("agents/invitations/{code}")]
        [ProducesResponseType(typeof(AgentInvitationInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetInvitationData(string code)
        {
            var (_, isFailure, invitationInfo, error) = await _agentInvitationService
                .GetPendingInvitation(code);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(invitationInfo);
        }


        /// <summary>
        ///     Gets current agent.
        /// </summary>
        /// <returns>Current agent information.</returns>
        [HttpGet("agents")]
        [ProducesResponseType(typeof(AgentDescription), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetCurrentAgent()
        {
            var (_, isFailure, agentInfo, error) = await _agentContextInternal.GetAgentInfo();
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(new AgentDescription(agentInfo.Email,
                agentInfo.LastName,
                agentInfo.FirstName,
                agentInfo.Title,
                agentInfo.Position,
                await _agentContextService.GetAgentCounterparties()));
        }
        

        /// <summary>
        ///     Updates current agent properties.
        /// </summary>
        [HttpPut("agents")]
        [ProducesResponseType(typeof(AgentEditableInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateCurrentAgent([FromBody] AgentEditableInfo newInfo)
        {
            var agentRegistrationInfo = await _agentService.UpdateCurrentAgent(newInfo);
            return Ok(agentRegistrationInfo);
        }


        /// <summary>
        ///     Gets all agents of an agency
        /// </summary>
        [HttpGet("agencies/{agencyId}/agents")]
        [ProducesResponseType(typeof(List<SlimAgentInfo>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.ReadOnly)]
        [InAgencyPermissions(InAgencyPermissions.ObserveAgents)]
        public async Task<IActionResult> GetAgents(int agencyId)
        {
            var (_, isFailure, agents, error) = await _agentService.GetAgents(agencyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(agents);
        }


        /// <summary>
        ///     Gets agent of a specified agency
        /// </summary>
        [HttpGet("agencies/{agencyId}/agents/{agentId}")]
        [ProducesResponseType(typeof(AgentInfoInAgency), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.ReadOnly)]
        [InAgencyPermissions(InAgencyPermissions.PermissionManagement)]
        public async Task<IActionResult> GetAgent(int agencyId, int agentId)
        {
            var (_, isFailure, agent, error) = await _agentService.GetAgent(agencyId, agentId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(agent);
        }


        /// <summary>
        ///     Updates permissions of a agent of a specified agency
        /// </summary>
        [HttpPut("agencies/{agencyId}/agents/{agentId}/permissions")]
        [ProducesResponseType(typeof(List<InAgencyPermissions>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.ReadOnly)]
        [InAgencyPermissions(InAgencyPermissions.PermissionManagement)]
        public async Task<IActionResult> UpdatePermissionsInAgency(int agencyId, int agentId,
            [FromBody] List<InAgencyPermissions> newPermissions)
        {
            var (_, isFailure, permissions, error) = await _permissionManagementService
                .SetInAgencyPermissions(agencyId, agentId, newPermissions);

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
        [HttpPut("agents/settings/application")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [AgentRequired]
        public async Task<IActionResult> SetApplicationSettings([FromBody] JToken settings)
        {
            var agentInfo = await _agentContextService.GetAgent();
            await _agentSettingsManager.SetAppSettings(agentInfo, settings);
            return NoContent();
        }


        /// <summary>
        ///     Gets user frontend application settings.
        /// </summary>
        /// <returns>Settings in dynamic JSON-format</returns>
        [HttpGet("agents/settings/application")]
        [ProducesResponseType(typeof(JToken), (int) HttpStatusCode.OK)]
        [AgentRequired]
        public async Task<IActionResult> GetApplicationSettings()
        {
            var agentInfo = await _agentContextService.GetAgent();
            var settings = await _agentSettingsManager.GetAppSettings(agentInfo);
            return Ok(settings);
        }


        /// <summary>
        ///     Sets user preferences.
        /// </summary>
        /// <param name="settings">Settings in JSON-format</param>
        /// <returns></returns>
        [HttpPut("agents/settings/user")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [AgentRequired]
        public async Task<IActionResult> SetUserSettings([FromBody] AgentUserSettings settings)
        {
            var agentInfo = await _agentContextService.GetAgent();
            await _agentSettingsManager.SetUserSettings(agentInfo, settings);
            return NoContent();
        }


        /// <summary>
        ///     Gets user preferences.
        /// </summary>
        /// <returns>Settings in JSON-format</returns>
        [HttpGet("agents/settings/user")]
        [ProducesResponseType(typeof(AgentUserSettings), (int) HttpStatusCode.OK)]
        [AgentRequired]
        public async Task<IActionResult> GetUserSettings()
        {
            var agentInfo = await _agentContextService.GetAgent();
            var settings = await _agentSettingsManager.GetUserSettings(agentInfo);
            return Ok(settings);
        }


        /// <summary>
        ///     Gets all possible permissions
        /// </summary>
        /// <returns> Array of all permission names </returns>
        [HttpGet("all-permissions-list")]
        [ProducesResponseType(typeof(IEnumerable<InAgencyPermissions>), (int)HttpStatusCode.OK)]
        [MinCounterpartyState(CounterpartyStates.ReadOnly)]
        public IActionResult GetAllPermissionsList() => Ok(InAgencyPermissions.All.ToList().Where(p => p != InAgencyPermissions.All));
        

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


        private readonly IAgentContextService _agentContextService;
        private readonly IAgentContextInternal _agentContextInternal;
        private readonly IAgentInvitationService _agentInvitationService;
        private readonly IAgentRegistrationService _agentRegistrationService;
        private readonly IAgentSettingsManager _agentSettingsManager;
        private readonly IAgentPermissionManagementService _permissionManagementService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAgentService _agentService;
        private readonly ITokenInfoAccessor _tokenInfoAccessor;
    }
}