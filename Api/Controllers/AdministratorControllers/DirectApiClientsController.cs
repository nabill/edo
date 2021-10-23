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
        /// Creates new direct api client in identity service
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType((int) StatusCodes.Status204NoContent)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> AddApiClient([FromBody] CreateDirectApiClientRequest request)
        {
            return NoContentOrBadRequest(await _directApiClientManagementService.AddApiClient(request));
        }


        /// <summary>
        /// Deletes direct api client from identity service
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType((int) StatusCodes.Status204NoContent)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> Delete([FromBody] RemoveDirectApiClientRequest request)
        {
            return NoContentOrBadRequest(await _directApiClientManagementService.RemoveApiClient(request));
        }


        /// <summary>
        /// Changes password for direct api client
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPut("{clientId}/change-password")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType((int) StatusCodes.Status204NoContent)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> ChangePassword(string clientId, [FromBody] string password)
        {
            return NoContentOrBadRequest(await _directApiClientManagementService.ChangePassword(clientId, password));
        }


        private readonly IDirectApiClientManagementService _directApiClientManagementService;
    }
}