using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/internal/bookings")]
    public class InternalBookingsController : BaseController
    {
        public InternalBookingsController(IAccommodationService accommodationService)
        {
            _accommodationService = accommodationService;
        }


        /// <summary>
        ///     Gets bookings for cancellation by deadline date
        /// </summary>
        /// <param name="deadlineDate">Deadline date</param>
        /// <returns>List of booking ids for cancellation</returns>
        [HttpGet("cancel/{deadlineDate}")]
        [ProducesResponseType(typeof(List<int>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetBookingsForCancellation(DateTime deadlineDate)
        {
            return OkOrBadRequest(await _accommodationService.GetBookingsForCancellation(deadlineDate));
        }


        /// <summary>
        ///     Cancels bookings
        /// </summary>
        /// <param name="bookingIds">List of booking ids for cancellation</param>
        /// <returns>Result message</returns>
        [HttpPost("cancel")]
        [ProducesResponseType(typeof(ProcessResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CancelBookings(List<int> bookingIds)
        {
            return OkOrBadRequest(await _accommodationService.CancelBookings(bookingIds));
        }

        private readonly IAccommodationService _accommodationService;
    }
}