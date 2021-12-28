using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Filters.Authorization.AgencyVerificationStatesFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AuxiliaryControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/accommodations/")]
    [Produces("application/json")]
    
    public class Accommodations : BaseController
    {
        public Accommodations(IAccommodationMapperClient mapperClient,
            IAgentContextService agentContextService,
            IAccommodationBookingSettingsService accommodationBookingSettingsService)
        {
            _mapperClient = mapperClient;
            _agentContextService = agentContextService;
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
        }
        
        /// <summary>
        /// Returns accommodation info by htId
        /// </summary>
        /// <param name="htId"></param>
        /// <returns></returns>
        [HttpGet("{htId}")]
        [ProducesResponseType(typeof(Accommodation), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [MinAgencyVerificationState(AgencyVerificationStates.ReadOnly)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationAvailabilitySearch)]
        public async Task<IActionResult> GetAccommodation([FromRoute] string htId)
        {
            var (_, isFailure, accommodation, error) = await _mapperClient.GetAccommodation(htId, LanguageCode);

            if (isFailure)
                return BadRequest(error);

            var agent = await _agentContextService.GetAgent();
            var searchSettings = await _accommodationBookingSettingsService.Get(agent);
            
            return Ok(accommodation.ToEdoContract().ToAgentAccommodation(searchSettings.IsSupplierVisible));
        }

        
        private readonly IAccommodationMapperClient _mapperClient;
        private readonly IAgentContextService _agentContextService;
        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
    }
}