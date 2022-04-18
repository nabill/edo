using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Models.Settings;
using HappyTravel.Edo.Api.Services.Files;
using HappyTravel.Edo.Api.Services.Locations;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/agencies")]
    [Produces("application/json")]
    public class AgenciesController : BaseController
    {
        public AgenciesController(IAgencySystemSettingsManagementService systemSettingsManagementService,
            IAgentService agentService,
            IAdminAgencyManagementService agencyManagementService,
            IAgencyVerificationService agencyVerificationService,
            IContractFileManagementService contractFileManagementService,
            ILocalityInfoService localityInfoService,
            IAgencyRemovalService agencyRemovalService)
        {
            _systemSettingsManagementService = systemSettingsManagementService;
            _agentService = agentService;
            _agencyManagementService = agencyManagementService;
            _agencyVerificationService = agencyVerificationService;
            _contractFileManagementService = contractFileManagementService;
            _localityInfoService = localityInfoService;
            _agencyRemovalService = agencyRemovalService;
        }


        /// <summary>
        ///     Gets agency's availability search settings
        /// </summary>
        /// <param name="agencyId">Agency Id</param>
        /// <returns>Agency availability search settings</returns>
        [HttpGet("{agencyId}/system-settings/availability-search")]
        [ProducesResponseType(typeof(AgencyAccommodationBookingSettingsInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [AdministratorPermissions(AdministratorPermissions.AgencyManagement)]
        public async Task<IActionResult> GetSystemSettings([FromRoute] int agencyId)
        {
            var (_, isFailure, settings, error) = await _systemSettingsManagementService.GetAvailabilitySearchSettings(agencyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            if (settings == default)
                return NoContent();

            return Ok(settings.ToAgencyAccommodationBookingSettingsInfo());
        }


        /// <summary>
        ///     Updates agency's availability search settings
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="agencyId">Agency Id</param>
        /// <returns></returns>
        [HttpPut("{agencyId}/system-settings/availability-search")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgencyManagement)]
        public async Task<IActionResult> SetSystemSettings([FromBody] AgencyAccommodationBookingSettingsInfo settings, [FromRoute] int agencyId)
            => NoContentOrBadRequest(await _systemSettingsManagementService.SetAvailabilitySearchSettings(agencyId, settings));


        /// <summary>
        ///     Deletes agency's availability search settings
        /// </summary>
        /// <param name="agencyId">Agency Id</param>
        /// <returns></returns>
        [HttpDelete("{agencyId}/system-settings/availability-search")]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [AdministratorPermissions(AdministratorPermissions.AgencyManagement)]
        public async Task<IActionResult> DeleteSystemSettings([FromRoute] int agencyId)
            => NoContentOrBadRequest(await _systemSettingsManagementService.DeleteAvailabilitySearchSettings(agencyId));


        /// <summary>
        ///     Gets a list of agents in the agency
        /// </summary>
        /// <param name="agencyId">Agency Id</param>
        /// <returns>List of agents</returns>
        [HttpGet("{agencyId}/agents")]
        [ProducesResponseType(typeof(List<SlimAgentInfo>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.ViewAgents)]
        public async Task<IActionResult> GetAgents([FromRoute] int agencyId)
        {
            var (_, isFailure, agents, error) = await _agentService.GetAgents(agencyId);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(agents);
        }


        /// <summary>
        ///     Gets child agencies.
        /// </summary>
        /// <returns></returns>
        [HttpGet("{agencyId}/child-agencies")]
        [ProducesResponseType(typeof(List<AgencyInfo>), (int)HttpStatusCode.OK)]
        [AdministratorPermissions(AdministratorPermissions.ViewAgencies)]
        public async Task<IActionResult> GetChildAgencies([FromRoute] int agencyId)
            => Ok(await _agencyManagementService.GetChildAgencies(agencyId, LanguageCode));


        /// <summary>
        ///  Deactivates specified agency.
        /// </summary>
        /// <param name="agencyId">Id of the agency.</param>
        /// <param name="request">Request data for deactivation.</param>
        /// <returns></returns>
        [HttpPost("{agencyId}/deactivate")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgencyManagement)]
        public async Task<IActionResult> DeactivateAgency(int agencyId, ActivityStatusChangeRequest request)
        {
            var (_, isFailure, error) = await _agencyManagementService.DeactivateAgency(agencyId, request.Reason);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///  Activates specified agency.
        /// </summary>
        /// <param name="agencyId">Id of the agency.</param>
        /// <param name="request">Request data for activation.</param>
        /// <returns></returns>
        [HttpPost("{agencyId}/activate")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgencyManagement)]
        public async Task<IActionResult> ActivateAgency(int agencyId, ActivityStatusChangeRequest request)
        {
            var (_, isFailure, error) = await _agencyManagementService.ActivateAgency(agencyId, request.Reason);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///  Gets specified agency.
        /// </summary>
        /// <param name="agencyId">Id of the agency.</param>
        [HttpGet("{agencyId}")]
        [ProducesResponseType(typeof(AgencyInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.ViewAgencies)]
        public async Task<IActionResult> GetAgency(int agencyId)
            => OkOrBadRequest(await _agencyManagementService.Get(agencyId, LanguageCode));


        /// <summary>
        ///     Sets agency verified read only.
        /// </summary>
        /// <param name="agencyId">Id of the agency to verify.</param>
        /// <param name="request">Verification details.</param>
        /// <returns></returns>
        [HttpPost("{agencyId}/verify-read-only")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgencyVerification)]
        public async Task<IActionResult> VerifyReadOnly(int agencyId, [FromBody] AgencyReadOnlyVerificationRequest request)
        {
            var (isSuccess, _, error) = await _agencyVerificationService.VerifyAsReadOnly(agencyId, request.Reason);

            return isSuccess
                ? (IActionResult)NoContent()
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        ///     Sets agency fully verified.
        /// </summary>
        /// <param name="agencyId">Id of the agency to verify.</param>
        /// <param name="request">Verification details.</param>
        /// <returns></returns>
        [HttpPost("{agencyId}/verify-full-access")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgencyVerification)]
        public async Task<IActionResult> VerifyFullAccess(int agencyId, [FromBody] AgencyFullAccessVerificationRequest request)
        {
            var (isSuccess, _, error) = await _agencyVerificationService.VerifyAsFullyAccessed(agencyId, request.ContractKind, request.Reason);

            return isSuccess
                ? (IActionResult)NoContent()
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        ///     Sets agency declined verification.
        /// </summary>
        /// <param name="agencyId">Id of the agency to verify.</param>
        /// <param name="request">Verification details.</param>
        /// <returns></returns>
        [HttpPost("{agencyId}/decline-verification")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgencyVerification)]
        public async Task<IActionResult> DeclineVerification(int agencyId, [FromBody] AgencyDeclinedVerificationRequest request)
        {
            var (isSuccess, _, error) = await _agencyVerificationService.DeclineVerification(agencyId, request.Reason);

            return isSuccess
                ? (IActionResult)NoContent()
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        /// Uploads a contract pdf file
        /// </summary>
        /// <param name="agencyId">Id of the agency to save the contract file for</param>
        /// <param name="file">A pdf file of the contract with the given agency</param>
        [HttpPut("{agencyId}/contract-file")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgencyManagement)]
        public async Task<IActionResult> UploadContractFile(int agencyId, [FromForm] IFormFile file)
        {
            var result = await _contractFileManagementService.Add(agencyId, file);

            return OkOrBadRequest(result);
        }


        /// <summary>
        /// Downloads a contract pdf file
        /// </summary>
        /// <param name="agencyId">Id of the agency to load the contract file for</param>
        [HttpGet("{agencyId}/contract-file")]
        [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgencyManagement)]
        public async Task<IActionResult> GetContractFile(int agencyId)
        {
            var (_, isFailure, (stream, contentType), error) = await _contractFileManagementService.Get(agencyId);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return File(stream, contentType);
        }


        /// <summary>
        /// Edits an agency with specified id
        /// </summary>
        /// <param name="agencyId">Id of the edited agency</param>
        /// <param name="request">New fields for the edited agency</param>
        [HttpPut("{agencyId}")]
        [ProducesResponseType(typeof(AgencyInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.AgencyManagement)]
        public async Task<IActionResult> EditAgency([FromRoute] int agencyId, [FromBody] ManagementEditAgencyRequest request)
        {
            var localityId = request.LocalityHtId;
            if (string.IsNullOrWhiteSpace(localityId))
                return BadRequest(ProblemDetailsBuilder.Build("Locality id is required"));

            var (_, isLocalityFailure, localityInfo, _) = await _localityInfoService.GetLocalityInfo(request.LocalityHtId);
            if (isLocalityFailure)
                return BadRequest(ProblemDetailsBuilder.Build("Locality doesn't exist"));

            var (_, isFailure, agency, error) = await _agencyManagementService.Edit(agencyId, request, localityInfo, LanguageCode);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(agency);
        }


        /// <summary>
        /// Gets a list of all root agencies
        /// </summary>
        [HttpGet("root-agencies")]
        [ProducesResponseType(typeof(List<AdminViewAgencyInfo>), (int)HttpStatusCode.OK)]
        [AdministratorPermissions(AdministratorPermissions.ViewAgencies)]
        [EnableQuery(PageSize = 500, MaxTop = 500)]
        public IEnumerable<AdminViewAgencyInfo> GetRootAgencies()
            => _agencyManagementService.GetRootAgencies(LanguageCode);


        [HttpDelete("{agencyId}")]
        [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.NoContent)]
        [AdministratorPermissions(AdministratorPermissions.AgencyManagement)]
        public async Task<IActionResult> Delete([FromRoute] int agencyId)
            => NoContentOrBadRequest(await _agencyRemovalService.Delete(agencyId));


        private readonly IAgencySystemSettingsManagementService _systemSettingsManagementService;
        private readonly IAgentService _agentService;
        private readonly IAdminAgencyManagementService _agencyManagementService;
        private readonly IAgencyVerificationService _agencyVerificationService;
        private readonly IContractFileManagementService _contractFileManagementService;
        private readonly ILocalityInfoService _localityInfoService;
        private readonly IAgencyRemovalService _agencyRemovalService;
    }
}