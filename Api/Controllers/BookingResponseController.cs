using System;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.ProviderResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}")]
    [Produces("application/json")]
    public class BookingResponseController : BaseController
    {
        public BookingResponseController(INetstormingResponseService netstormingResponseService)
        {
            _netstormingResponseService = netstormingResponseService;
        }
        
        
        [AllowAnonymous]
        [HttpPost("bookings/accommodations/responses/netstorming")]
        public async Task<IActionResult> HandleNetstormingBookingResponse()
        {
            var (_, isFailure, xmlBody, error) = await RequestHelper.GetAsString(HttpContext.Request.Body);
            if (isFailure)
                return BadRequest(new ProblemDetails
                {
                    Detail = error,
                    Status = (int) HttpStatusCode.BadRequest
                });
            await _netstormingResponseService.HandleBookingResponse(xmlBody);
            return Ok("");
        }


        private readonly INetstormingResponseService _netstormingResponseService;
    }
}