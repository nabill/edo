using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Services.Reports;
using Microsoft.AspNetCore.Authorization;
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
        public ReportsController(IDirectConnectivityReportService directConnectivityReportService, IAgenciesProductionReportService agenciesProductionReportService)
        {
            _directConnectivityReportService = directConnectivityReportService;
            _agenciesProductionReportService = agenciesProductionReportService;
        }
        
        
        /// <summary>
        ///     Returns supplier wise direct connectivity report
        /// </summary>
        [HttpGet("direct-connectivity-report/supplier-wise")]
        [ProducesResponseType(typeof(FileStream), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.DirectConnectivityReport)]
        public async Task<IActionResult> GetSupplerWiseDirectConnectivityReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _directConnectivityReportService.GetSupplierWiseReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"direct-connectivity-report-suppliers-{from:g}-{end:g}.csv"
            };
        }
        
        
        /// <summary>
        ///     Returns agency wise direct connectivity report
        /// </summary>
        [HttpGet("direct-connectivity-report/agency-wise")]
        [ProducesResponseType(typeof(FileStream), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.DirectConnectivityReport)]
        public async Task<IActionResult> GetAgencyWiseDirectConnectivityReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _directConnectivityReportService.GetAgencyWiseReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"direct-connectivity-report-agencies-{from:g}-{end:g}.csv"
            };
        }
        
        
        /// <summary>
        ///     Returns agencies production report
        /// </summary>
        [HttpGet("agencies-production-report")]
        [ProducesResponseType(typeof(FileStream), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgenciesProductionReport)]
        public async Task<IActionResult> GetAgenciesProductionReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _agenciesProductionReportService.GetReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"agencies-production-report-{from:g}-{end:g}.csv"
            };
        }
        
        
        private readonly IDirectConnectivityReportService _directConnectivityReportService;
        private readonly IAgenciesProductionReportService _agenciesProductionReportService;
    }
}