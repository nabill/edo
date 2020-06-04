using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.ServiceAccountFilters;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Management;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/internal/bookings")]
    public class InternalBookingsController : BaseController
    {
        public InternalBookingsController(IBookingsProcessingService bookingsProcessingService,
            IServiceAccountContext serviceAccountContext)
        {
            _bookingsProcessingService = bookingsProcessingService;
            _serviceAccountContext = serviceAccountContext;
        }


        /// <summary>
        ///     Gets bookings for cancellation
        /// </summary>
        /// <returns>List of booking ids for cancellation</returns>
        [HttpGet("cancel")]
        [ProducesResponseType(typeof(List<int>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> GetBookingsForCancellation()
            => Ok(await _bookingsProcessingService.GetForCancellation());


        /// <summary>
        ///     Cancels bookings
        /// </summary>
        /// <param name="bookingIds">List of booking ids for cancellation</param>
        /// <returns>Result message</returns>
        [HttpPost("cancel")]
        [ProducesResponseType(typeof(ProcessResult), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> CancelBookings(List<int> bookingIds)
        {
            var (_, _, serviceAccount, _) = await _serviceAccountContext.GetCurrent();
            return OkOrBadRequest(await _bookingsProcessingService.Cancel(bookingIds, serviceAccount));
        }
        
        /// <summary>
        ///     Gets bookings for payment completion by deadline date
        /// </summary>
        /// <param name="deadlineDate">Deadline date</param>
        /// <returns>List of booking ids for capture</returns>
        [HttpGet("capture/{deadlineDate}")]
        [ProducesResponseType(typeof(List<int>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> GetBookingsForCapture(DateTime? deadlineDate)
        {
            if (!deadlineDate.HasValue)
                return BadRequest($"Deadline date should be specified");
            
            return Ok(await _bookingsProcessingService.GetForCapture(deadlineDate.Value));
        }


        /// <summary>
        ///     Captures payments for bookings
        /// </summary>
        /// <param name="bookingIds">List of booking ids for capture</param>
        /// <returns>Result message</returns>
        [HttpPost("capture")]
        [ProducesResponseType(typeof(ProcessResult), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> Capture(List<int> bookingIds)
        {
            var (_, _, serviceAccount, _) = await _serviceAccountContext.GetCurrent();
            return OkOrBadRequest(await _bookingsProcessingService.Capture(bookingIds, serviceAccount));
        }


        /// <summary>
        ///     Sends need payment notifications for bookings
        /// </summary>
        /// <param name="deadlineDate">Deadline date</param>
        /// <returns>Result message</returns>
        [HttpPost("notify/need-payment/{deadlineDate}")]
        [ProducesResponseType(typeof(List<int>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> GetBookingsToNotify(DateTime? deadlineDate)
        {
            if (!deadlineDate.HasValue)
                return BadRequest($"Deadline date should be specified");
            
            return Ok(await _bookingsProcessingService.GetForNotification(deadlineDate.Value));
        }


        /// <summary>
        ///     Sends need payment notifications for bookings
        /// </summary>
        /// <param name="bookingIds">List of booking ids for notify</param>
        /// <returns>Result message</returns>
        [HttpPost("notify/need-payment")]
        [ProducesResponseType(typeof(ProcessResult), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> NotifyPaymentsNeeded(List<int> bookingIds)
        {
            var (_, _, serviceAccount, _) = await _serviceAccountContext.GetCurrent();
            return OkOrBadRequest(await _bookingsProcessingService.NotifyDeadlineApproaching(bookingIds, serviceAccount));
        }


        private readonly IBookingsProcessingService _bookingsProcessingService;
        private readonly IServiceAccountContext _serviceAccountContext;
    }
}