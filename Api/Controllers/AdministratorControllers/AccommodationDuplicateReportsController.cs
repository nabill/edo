using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Services.Management;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/accommodation-duplicate-reports")]
    [Produces("application/json")]
    public class AccommodationDuplicateReportsController : BaseController
    {
        public AccommodationDuplicateReportsController(IAccommodationDuplicateReportsManagementService reportsManagementService,
            IAdministratorContext administratorContext)
        {
            _reportsManagementService = reportsManagementService;
            _administratorContext = administratorContext;
        }


        /// <summary>
        /// Gets specified counterparty.
        /// </summary>
        /// <returns>List of all duplicate reports</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<SlimAccommodationDuplicateReportInfo>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AccommodationDuplicatesReportApproval)]
        [EnableQuery]
        public Task<List<SlimAccommodationDuplicateReportInfo>> Get()
        {
            // TODO: Replace with in-database filtering
            return _reportsManagementService.Get();
        }
        
        
        /// <summary>
        /// Gets specified counterparty.
        /// </summary>
        /// <returns>List of all duplicate reports</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(List<SlimAccommodationDuplicateReportInfo>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AccommodationDuplicatesReportApproval)]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            var (_, isFailure, reportInfo, error) = await _reportsManagementService.Get(id, LanguageCode);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(reportInfo);
        }


        /// <summary>
        /// Approves duplicate report by given id.
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        [HttpGet("{reportId}/approve")]
        [ProducesResponseType(typeof(List<CounterpartyInfo>), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AccommodationDuplicatesReportApproval)]
        public async Task<IActionResult> Approve(int reportId)
        {
            var (_, _, admin, _) = await _administratorContext.GetCurrent();
            var (_, isFailure, error) = await _reportsManagementService.Approve(reportId, admin);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        
        /// <summary>
        /// Approves duplicate report by given id.
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        [HttpGet("{reportId}/disapprove")]
        [ProducesResponseType(typeof(List<CounterpartyInfo>), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AccommodationDuplicatesReportApproval)]
        public async Task<IActionResult> Disapprove(int reportId)
        {
            var (_, _, admin, _) = await _administratorContext.GetCurrent();
            var (_, isFailure, error) = await _reportsManagementService.Disapprove(reportId, admin);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        private readonly IAccommodationDuplicateReportsManagementService _reportsManagementService;
        private readonly IAdministratorContext _administratorContext;
    }
}