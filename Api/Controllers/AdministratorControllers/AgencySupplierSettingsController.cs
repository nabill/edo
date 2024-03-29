using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/{v:apiVersion}/admin/agencies/")]
[Produces("application/json")]
public class AgencySupplierSettingsController : BaseController
{
    public AgencySupplierSettingsController(IAgencySupplierManagementService agencySupplierManagementService)
    {
        _agencySupplierManagementService = agencySupplierManagementService;
    }


    [HttpGet("{agencyId}/suppliers")]
    [ProducesResponseType(typeof(Dictionary<string, bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
    public async Task<IActionResult> Get(int agencyId)
        => OkOrBadRequest(await _agencySupplierManagementService.GetMaterializedSuppliers(agencyId));


    [HttpPut("{agencyId}/suppliers")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [AdministratorPermissions(AdministratorPermissions.AgentManagement)]
    public async Task<IActionResult> Put(int agencyId, Dictionary<string, bool> enabledSuppliers)
        => NoContentOrBadRequest(await _agencySupplierManagementService.SaveSuppliers(agencyId, enabledSuppliers));


    private readonly IAgencySupplierManagementService _agencySupplierManagementService;
}