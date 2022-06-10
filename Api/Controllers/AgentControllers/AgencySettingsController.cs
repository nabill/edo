using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/agency")]
    [Produces("application/json")]
    public class AgencySettingsController : BaseController
    {
        public AgencySettingsController(IAccommodationBookingSettingsService accommodationBookingSettingsService)
        {
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
        }


        /// <summary>
        ///     Gets Advanced Purchase Rates settings for an agency.
        /// </summary>
        /// <returns></returns>
        [HttpGet("system-settings/apr-settings")]
        [ProducesResponseType(typeof(AprMode), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAdvancedPurchaseRatesSettings()
        {
            return Ok((await _accommodationBookingSettingsService.Get()).AprMode);
        }

        
        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
    }
}
