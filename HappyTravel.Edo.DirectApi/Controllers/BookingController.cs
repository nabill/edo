using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.DirectApi.Models;
using HappyTravel.Edo.DirectApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.DirectApi.Controllers
{
    /// <summary>
    /// <h2>Booking management</h2>
    /// </summary>
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/{version:apiVersion}/bookings")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class BookingController : ControllerBase
    {
        public BookingController(IAgentContextService agentContextService, BookingCancellationService bookingCancellationService, BookingInfoService bookingInfoService, BookingCreationService bookingCreationService)
        {
            _agentContextService = agentContextService;
            _bookingCancellationService = bookingCancellationService;
            _bookingInfoService = bookingInfoService;
            _bookingCreationService = bookingCreationService;
        }
        
        
        
        /// <summary>
        /// Register booking.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Booking>> Register([FromBody] AccommodationBookingRequest request)
        {
            var agent = await _agentContextService.GetAgent();

            var (isSuccess, _, booking, error) = await _bookingCreationService.Register(request, agent, "en");
            
            return isSuccess
                ? Ok(booking)
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        /// Finalize booking
        /// </summary>>
        [HttpPost("finalize")]
        public async Task<ActionResult<Booking>> Finalize([FromBody] BookingFinalizationRequest request)
        {
            var agent = await _agentContextService.GetAgent();

            var (isSuccess, _, booking, error) = await _bookingCreationService.Finalize(request, agent, "en");
            
            return isSuccess
                ? Ok(booking)
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        /// Get booking info
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<Booking>> Get(string? referenceCode, string? supplierReferenceCode)
        {
            var agent = await _agentContextService.GetAgent();
            var (isSuccess, _, booking, error) = await _bookingInfoService.Get(referenceCode, supplierReferenceCode, agent);
            
            return isSuccess
                ? Ok(booking.FromEdoModels())
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        /// Get bookings
        /// </summary>
        [HttpGet("list")]
        public async Task<ActionResult<List<Booking>>> GetList(DateTime from, DateTime to)
        {
            var agent = await _agentContextService.GetAgent();
            return await _bookingInfoService.Get(from, to, agent);
        }


        /// <summary>
        /// Cancel booking
        /// </summary>
        [HttpPost("{referenceCode}/cancel")]
        public async Task<IActionResult> Cancel(string referenceCode)
        {
            var agent = await _agentContextService.GetAgent();
            var (isSuccess, _, error) = await _bookingCancellationService.Cancel(referenceCode, agent);
            
            return isSuccess
                ? NoContent()
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }
        
        
        private readonly IAgentContextService _agentContextService;
        private readonly BookingCancellationService _bookingCancellationService;
        private readonly BookingInfoService _bookingInfoService;
        private readonly BookingCreationService _bookingCreationService;
    }
}