using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Agents;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/agent-roles")]
    [Produces("application/json")]
    public class AgentRolesController : BaseController
    {
        public AgentRolesController(IAgentRolesService agentRolesService)
        {
            _agentRolesService = agentRolesService;
        }


        /// <summary>
        ///    Returns all agent roles
        /// </summary>
        /// <returns>Agent roles</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll()
        {
            var allRoles = await _agentRolesService.GetAll();
            return Ok(allRoles);
        }


        private readonly IAgentRolesService _agentRolesService;
    }
}