using System;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Reports;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/agent/reports")]
    [Produces("application/json")]
    public class AgentReportsController : BaseController
    {
        public AgentReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }
        
        
        /// <summary>
        ///     Returns agent wise report data
        /// </summary>
        [HttpGet("agent-wise-report")]
        [ProducesResponseType(typeof(FileStream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationBooking)]
        public async Task<IActionResult> GetAgentWiseReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _reportService.GetAgentWiseReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"agent-wise-report-{from:g}-{end:g}.csv"
            };
        }
        
        
        private readonly IReportService _reportService;
    }
}