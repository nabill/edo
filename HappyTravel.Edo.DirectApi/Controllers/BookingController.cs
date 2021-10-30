﻿using System;
using System.Threading.Tasks;
using HappyTravel.Edo.DirectApi.Models;
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
        /// <summary>
        /// Creating booking.
        /// </summary>
        [HttpPost("book")]
        public async Task<IActionResult> Book([FromBody] AccommodationBookingRequest request)
        {
            return Ok();
        }


        /// <summary>
        /// Get booking info
        /// </summary>
        [HttpGet("{referenceCode}")]
        public async Task<IActionResult> Get(string referenceCode)
        {
            return Ok();
        }


        /// <summary>
        /// Get bookings
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetList(DateTime from, DateTime to)
        {
            return Ok();
        }


        /// <summary>
        /// Cancel booking
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Cancel()
        {
            return Ok();
        }
    }
}