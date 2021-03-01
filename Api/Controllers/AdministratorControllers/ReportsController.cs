using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Services.Documents;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/reports")]
    [Produces("application/json")]
    public class ReportsController : BaseController
    {
        public ReportsController(IDirectConnectivityReportService directConnectivityReportService)
        {
            _directConnectivityReportService = directConnectivityReportService;
        }
        
        
        /// <summary>
        ///     Returns supplier wise direct connectivity report
        /// </summary>
        [HttpGet("direct-connectivity-report/supplier-wise")]
        [ProducesResponseType(typeof(FileStream), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.DirectConnectivityReport)]
        public async Task<IActionResult> GetSupplerWiseDirectConnectivityReport(Suppliers supplier, DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _directConnectivityReportService.GetSupplierWiseReport(supplier, from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"direct-connectivity-report-{supplier}-{from:g}-{end:g}.csv"
            };
        }
        
        
        private readonly IDirectConnectivityReportService _directConnectivityReportService;
    }
}