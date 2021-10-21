using System.Threading.Tasks;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/direct-api-clients")]
    [Produces("application/json")]
    public class DirectApiClientsController : BaseController
    {
        public DirectApiClientsController(IDirectApiClientManagementService directApiClientManagementService)
        {
            _directApiClientManagementService = directApiClientManagementService;
        }


        /// <summary>
        /// Returns all existed direct api clients
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType((int) StatusCodes.Status201Created)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> GetAllClients()
        {
            return Ok(await _directApiClientManagementService.GetAllClients());
        }
        
        
        /// <summary>
        /// Returns direct api client by id
        /// </summary>
        /// <returns></returns>
        [HttpGet("{clientId}")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType((int) StatusCodes.Status201Created)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> GetClient(string clientId)
        {
            return OkOrBadRequest(await _directApiClientManagementService.GetById(clientId));
        }
        
        
        /// <summary>
        /// Creates new direct api client in identity service
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType((int) StatusCodes.Status201Created)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> Create([FromBody] CreateDirectApiClientRequest request)
        {
            return OkOrBadRequest(await _directApiClientManagementService.Create(request));
        }

        
        
        /// <summary>
        /// Deletes direct api client from identity service
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        [HttpDelete("{clientId}")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType((int) StatusCodes.Status201Created)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> Delete(string clientId)
        {
            return OkOrBadRequest(await _directApiClientManagementService.Delete(clientId));
        }


        /// <summary>
        /// Activates direct api client in identity service
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        [HttpPost("{clientId}/activate")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType((int) StatusCodes.Status201Created)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> Activate(string clientId)
        {
            return OkOrBadRequest(await _directApiClientManagementService.Activate(clientId));
        }


        /// <summary>
        /// Deactivates direct api client in identity service
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        [HttpPost("{clientId}/deactivate")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType((int) StatusCodes.Status201Created)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> Deactivate(string clientId)
        {
            return OkOrBadRequest(await _directApiClientManagementService.Deactivate(clientId));
        }


        /// <summary>
        /// Bind direct api client to agent entity
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="agentId"></param>
        /// <returns></returns>
        [HttpPost("{clientId}/bind/{agentId}")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType((int) StatusCodes.Status201Created)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> BindToAgent(string clientId, int agentId)
        {
            return OkOrBadRequest(await _directApiClientManagementService.BindToAgent(clientId, agentId));
        }


        /// <summary>
        /// Unbind direct api client from agent entity
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="agentId"></param>
        /// <returns></returns>
        [HttpPost("{clientId}/unbind/{agentId}")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType((int) StatusCodes.Status201Created)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> UnbindFromAgent(string clientId, int agentId)
        {
            return OkOrBadRequest(await _directApiClientManagementService.UnbindFromAgent(clientId, agentId));
        }


        private readonly IDirectApiClientManagementService _directApiClientManagementService;
    }
}