using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/{v:apiVersion}/agency/bonuses")]
[Produces("application/json")]
public class MarkupBonusesController : BaseController
{
    public MarkupBonusesController(IAgentContextService agentContext, IMarkupBonusDisplayService bonusDisplayService)
    {
        _agentContext = agentContext;
        _bonusDisplayService = bonusDisplayService;
    }
    
    /// <summary>
    ///     Gets list of applied markups for agency
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [InAgencyPermissions(InAgencyPermissions.ObserveMarkup)]
    [ProducesResponseType(typeof(List<Bonus>), (int) HttpStatusCode.OK)]
    [EnableQuery]
    public async Task<IActionResult> GetBonuses()
    {
        var agent = await _agentContext.GetAgent();
        return Ok(_bonusDisplayService.GetBonuses(agent));
    }

        
    /// <summary>
    ///     Gets summary amount of applied markups for agency
    /// </summary>
    /// <param name="filter">Filter for date range</param>
    /// <returns></returns>
    [HttpGet("sum")]
    [InAgencyPermissions(InAgencyPermissions.ObserveMarkup)]
    [ProducesResponseType(typeof(BonusSummary), (int) HttpStatusCode.OK)]
    public async Task<IActionResult> GetBonusesSummary([FromQuery] BonusSummaryFilter filter)
    {
        var agent = await _agentContext.GetAgent();
        return Ok(await _bonusDisplayService.GetBonusesSummary(filter, agent));
    }
    
    
    private readonly IAgentContextService _agentContext;
    private readonly IMarkupBonusDisplayService _bonusDisplayService;
}