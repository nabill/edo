using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Filters.Authorization.CounterpartyStatesFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping;
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
        public Accommodations(IAccommodationMapperClient mapperClient)
        {
            _mapperClient = mapperClient;
        }
        
        /// <summary>
        /// Returns accommodation info by htId
        /// </summary>
        /// <param name="htId"></param>
        /// <returns></returns>
        [HttpGet("{htId}")]
        [ProducesResponseType(typeof(Accommodations), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [MinCounterpartyState(CounterpartyStates.ReadOnly)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationAvailabilitySearch)]
        public async Task<IActionResult> GetAccommodation([FromRoute] string htId)
        {
            var (_, isFailure, accommodation, error) = await _mapperClient.GetAccommodation(htId, LanguageCode);

            if (isFailure)
                return BadRequest(error);
                
            return Ok(accommodation.ToEdoContract());
        }

        
        private readonly IAccommodationMapperClient _mapperClient;
    }
}