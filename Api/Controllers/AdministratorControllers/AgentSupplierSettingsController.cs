using System.Collections.Generic;
using System.Threading.Tasks;
using Api.AdministratorServices;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Controllers;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/agents/")]
    [Produces("application/json")]
    public class AgentSupplierSettingsController : BaseController
    {
        public AgentSupplierSettingsController(IAgentSupplierManagementService agentSupplierManagementService)
        {
            _agentSupplierManagementService = agentSupplierManagementService;
        }


        [HttpGet("{agentId}/suppliers")]
        [ProducesResponseType(typeof(Dictionary<string, bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> Get(int agentId)
            => OkOrBadRequest(await _agentSupplierManagementService.GetMaterializedSuppliers(agentId));


        [HttpPut("{agentId}/suppliers")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
        public async Task<IActionResult> Put(int agentId, Dictionary<string, bool> enabledSuppliers)
            => NoContentOrBadRequest(await _agentSupplierManagementService.SaveSuppliers(agentId, enabledSuppliers));


        private readonly IAgentSupplierManagementService _agentSupplierManagementService;
    }
}