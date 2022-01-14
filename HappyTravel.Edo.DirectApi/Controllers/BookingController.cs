using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.DirectApi.Models;
using HappyTravel.Edo.DirectApi.Models.Booking;
using HappyTravel.Edo.DirectApi.Services;
using HappyTravel.Edo.DirectApi.Services.Bookings;
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
        [HttpPost("{clientReferenceCode}/finalize")]
        public async Task<ActionResult<Booking>> Finalize([Required] string clientReferenceCode)
        {
            var agent = await _agentContextService.GetAgent();

            var (isSuccess, _, booking, error) = await _bookingCreationService.Finalize(clientReferenceCode, agent, "en");
            
            return isSuccess
                ? Ok(booking)
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        /// Get booking info
        /// </summary>
        [HttpGet("{clientReferenceCode}")]
        public async Task<ActionResult<Booking>> Get([Required] string clientReferenceCode)
        {
            var agent = await _agentContextService.GetAgent();
            var (isSuccess, _, booking, error) = await _bookingInfoService.Get(clientReferenceCode, agent);
            
            return isSuccess
                ? Ok(booking.FromEdoModels())
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }


        /// <summary>
        /// Get bookings
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<BookingSlim>>> GetList([FromQuery] BookingsListFilter filters)
        {
            var agent = await _agentContextService.GetAgent();
            return await _bookingInfoService.Get(filters, agent);
        }


        /// <summary>
        /// Cancel booking
        /// </summary>
        [HttpPost("{clientReferenceCode}/cancel")]
        public async Task<ActionResult<Booking>> Cancel(string clientReferenceCode)
        {
            var agent = await _agentContextService.GetAgent();
            var (isSuccess, _, booking, error) = await _bookingCancellationService.Cancel(clientReferenceCode, agent);
            
            return isSuccess
                ? booking
                : BadRequest(ProblemDetailsBuilder.Build(error));
        }
        
        
        private readonly IAgentContextService _agentContextService;
        private readonly BookingCancellationService _bookingCancellationService;
        private readonly BookingInfoService _bookingInfoService;
        private readonly BookingCreationService _bookingCreationService;
    }
}