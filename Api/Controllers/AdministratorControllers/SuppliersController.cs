using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Common.Enums.Administrators;
using HappyTravel.SupplierOptionsClient;
using HappyTravel.SupplierOptionsClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/{v:apiVersion}/admin/suppliers")]
[AdministratorPermissions(AdministratorPermissions.SupplierManagement)]
[Produces("application/json")]
public class SuppliersController : BaseController
{
    public SuppliersController(ISupplierOptionsClient supplierOptionsClient)
    {
        _supplierOptionsClient = supplierOptionsClient;
    }
    
    
    /// <summary>
    /// Gets all suppliers
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<SlimSupplier>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add()
        => OkOrBadRequest(await _supplierOptionsClient.GetAll());
    
    
    /// <summary>
    /// Gets supplier by code
    /// </summary>
    /// <param name="code">Supplier code</param>
    /// <returns></returns>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(RichSupplier), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get([FromRoute] string code)
        => OkOrBadRequest(await _supplierOptionsClient.Get(code));
    
    
    /// <summary>
    /// Adds a new supplier
    /// </summary>
    /// <param name="supplier">A new supplier info</param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [AdministratorPermissions(AdministratorPermissions.SupplierManagement)]
    public async Task<IActionResult> Add([FromBody] RichSupplier supplier)
        => NoContentOrBadRequest(await _supplierOptionsClient.Add(supplier));


    /// <summary>
    /// Modifies existing supplier
    /// </summary>
    /// <param name="code">Supplier code</param>
    /// <param name="supplier">Supplier data</param>
    [HttpPut("{code}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Modify([FromRoute] string code, [FromBody] RichSupplier supplier)
        => NoContentOrBadRequest(await _supplierOptionsClient.Modify(code, supplier));


    /// <summary>
    /// Activates a supplier
    /// </summary>
    /// <param name="code">Supplier code</param>
    /// <param name="reason">Reason for activating the supplier</param>
    [HttpPost("{code}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [AdministratorPermissions(AdministratorPermissions.AdministratorManagement)]
    public async Task<IActionResult> Activate([FromRoute] string code, [FromQuery] string reason)
        => NoContentOrBadRequest(await _supplierOptionsClient.Activate(code, reason));


    /// <summary>
    /// Deactivates a supplier
    /// </summary>
    /// <param name="code">Supplier code</param>
    /// <param name="reason">Reason for deactivating the supplier</param>
    [HttpPost("{code}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [AdministratorPermissions(AdministratorPermissions.AdministratorManagement)]
    public async Task<IActionResult> Deactivate([FromRoute] string code, [FromQuery] string reason)
        => NoContentOrBadRequest(await _supplierOptionsClient.Deactivate(code, reason));


    /// <summary>
    /// Sets enablement state of a supplier
    /// </summary>
    /// <param name="code">Supplier code</param>
    /// <param name="request">Reason for setting enablement state</param>
    /// <returns></returns>
    [HttpPut("{code}/set-enablement-state")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [AdministratorPermissions(AdministratorPermissions.AdministratorManagement)]
    public async Task<IActionResult> SetEnablementState([FromRoute] string code, [FromBody] SetEnablementStateRequest request) 
        => NoContentOrBadRequest(await _supplierOptionsClient.SetEnablementState(code, request.State, request.Reason));


    /// <summary>
    /// Gets supplier priorities
    /// </summary>
    [HttpGet("priorities")]
    [ProducesResponseType(typeof(SupplierPriorityByTypes), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [AdministratorPermissions(AdministratorPermissions.AdministratorManagement)]
    public async Task<IActionResult> GetPriorities()
        => OkOrBadRequest(await _supplierOptionsClient.GetPriorities());
    

    /// <summary>
    /// Modifies supplier priorities
    /// </summary>
    /// <param name="supplierPriorityByTypes">Supplier priorities</param>
    [HttpPut("priorities")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [AdministratorPermissions(AdministratorPermissions.AdministratorManagement)]
    public async Task<IActionResult> ModifyPriorities([FromBody] SupplierPriorityByTypes supplierPriorityByTypes)
        => NoContentOrBadRequest(await _supplierOptionsClient.ModifyPriorities(supplierPriorityByTypes));


    private readonly ISupplierOptionsClient _supplierOptionsClient;
}