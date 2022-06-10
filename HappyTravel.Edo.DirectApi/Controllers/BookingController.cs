using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using HappyTravel.Edo.DirectApi.Models.Booking;
using HappyTravel.Edo.DirectApi.Services.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.DirectApi.Controllers
{
    /// <summary>
    /// These endpoints allow you to make and manage bookings.
    /// </summary>
    [ApiController]
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/{version:apiVersion}/bookings/", Name = "Bookings", Order = 3)]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public class BookingController : BaseController
    {
        public BookingController(BookingCancellationService bookingCancellationService, BookingInfoService bookingInfoService, BookingCreationService bookingCreationService)
        {
            _bookingCancellationService = bookingCancellationService;
            _bookingInfoService = bookingInfoService;
            _bookingCreationService = bookingCreationService;
        }
        
        
        
        /// <summary>
        /// Register a booking
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Booking>> Register([FromBody] AccommodationBookingRequest request) 
            => OkOrBadRequest(await _bookingCreationService.Register(request));


        /// <summary>
        /// Finalize a booking
        /// </summary>>
        [HttpPost("{clientReferenceCode}/finalize")]
        public async Task<ActionResult<Booking>> Finalize([Required] string clientReferenceCode) 
            => OkOrBadRequest(await _bookingCreationService.Finalize(clientReferenceCode));


        /// <summary>
        /// Get booking info
        /// </summary>
        [HttpGet("{clientReferenceCode}")]
        public async Task<ActionResult<Booking>> Get([Required] string clientReferenceCode) 
            => OkOrBadRequest(await _bookingInfoService.GetConvertedBooking(clientReferenceCode));


        /// <summary>
        /// Get bookings
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<SlimBooking>>> GetList([FromQuery] BookingsListFilter filters) 
            => await _bookingInfoService.Get(filters);


        /// <summary>
        /// Cancel a booking
        /// </summary>
        [HttpPost("{clientReferenceCode}/cancel")]
        public async Task<ActionResult<Booking>> Cancel(string clientReferenceCode) 
            => OkOrBadRequest(await _bookingCancellationService.Cancel(clientReferenceCode));


        private readonly BookingCancellationService _bookingCancellationService;
        private readonly BookingInfoService _bookingInfoService;
        private readonly BookingCreationService _bookingCreationService;
    }
}