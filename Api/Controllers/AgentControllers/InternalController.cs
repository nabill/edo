using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Api.AdministratorServices;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.ServiceAccountFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BatchProcessing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Api.Services.Markups;
using HappyTravel.Edo.Api.Services.Payments.NGenius;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/internal")]
    public class InternalController : BaseController
    {
        public InternalController(IBookingsProcessingService bookingsProcessingService,
            IServiceAccountContext serviceAccountContext,
            IBookingReportsService reportsService,
            IMarkupBonusMaterializationService markupBonusMaterializationService,
            IBookingStatusRefreshService bookingRefreshStatusService,
            INGeniusRefundService refundService,
            IBalanceNotificationsService balanceNotificationService)
        {
            _bookingsProcessingService = bookingsProcessingService;
            _serviceAccountContext = serviceAccountContext;
            _reportsService = reportsService;
            _markupBonusMaterializationService = markupBonusMaterializationService;
            _bookingRefreshStatusService = bookingRefreshStatusService;
            _refundService = refundService;
            _balanceNotificationService = balanceNotificationService;
        }


        /// <summary>
        ///     Gets bookings for cancellation
        /// </summary>
        /// <returns>List of booking ids for cancellation</returns>
        [HttpGet("bookings/to-cancel")]
        [ProducesResponseType(typeof(List<int>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> GetBookingsForCancellation([FromQuery] DateTimeOffset? date)
        {
            if (date is null)
                return BadRequest(ProblemDetailsBuilder.Build("Date must be specified"));

            return Ok(await _bookingsProcessingService.GetForCancellation(date.Value));
        }


        /// <summary>
        ///     Cancels bookings
        /// </summary>
        /// <param name="bookingIds">List of booking ids for cancellation</param>
        /// <returns>Result message</returns>
        [HttpPost("bookings/cancel")]
        [ProducesResponseType(typeof(BatchOperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> CancelBookings(List<int> bookingIds)
        {
            var (_, _, serviceAccount, _) = await _serviceAccountContext.GetCurrent();
            return OkOrBadRequest(await _bookingsProcessingService.Cancel(bookingIds, serviceAccount));
        }

        /// <summary>
        ///     Gets bookings for payment completion by deadline date
        /// </summary>
        /// <param name="date">Deadline date</param>
        /// <returns>List of booking ids for capture</returns>
        [HttpGet("bookings/to-capture")]
        [ProducesResponseType(typeof(List<int>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> GetBookingsForCapture([FromQuery] DateTimeOffset? date)
        {
            if (date is null)
                return BadRequest(ProblemDetailsBuilder.Build("Date must be specified"));

            return Ok(await _bookingsProcessingService.GetForCapture(date.Value));
        }


        /// <summary>
        ///     Captures payments for bookings
        /// </summary>
        /// <param name="bookingIds">List of booking ids for capture</param>
        /// <returns>Result message</returns>
        [HttpPost("bookings/capture")]
        [ProducesResponseType(typeof(BatchOperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> Capture(List<int> bookingIds)
        {
            var (_, _, serviceAccount, _) = await _serviceAccountContext.GetCurrent();
            return OkOrBadRequest(await _bookingsProcessingService.Capture(bookingIds, serviceAccount));
        }


        /// <summary>
        ///     Gets bookings for payment charge by deadline date
        /// </summary>
        /// <param name="date">Deadline date</param>
        /// <returns>List of booking ids for charge</returns>
        [HttpGet("bookings/to-charge")]
        [ProducesResponseType(typeof(List<int>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> GetBookingsForCharge([FromQuery] DateTimeOffset? date)
        {
            if (!date.HasValue)
                return BadRequest(ProblemDetailsBuilder.Build($"Date should be specified"));

            return Ok(await _bookingsProcessingService.GetForCharge(date.Value));
        }


        /// <summary>
        ///     Charges payments for bookings
        /// </summary>
        /// <param name="bookingIds">List of booking ids for charge</param>
        /// <returns>Result message</returns>
        [HttpPost("bookings/charge")]
        [ProducesResponseType(typeof(BatchOperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> Charge(List<int> bookingIds)
        {
            var (_, _, serviceAccount, _) = await _serviceAccountContext.GetCurrent();
            return OkOrBadRequest(await _bookingsProcessingService.Charge(bookingIds, serviceAccount));
        }


        /// <summary>
        ///     Sends need payment notifications for bookings
        /// </summary>
        /// <param name="date">Deadline date</param>
        /// <returns>Result message</returns>
        [HttpGet("bookings/to-notify/deadline-approach")]
        [ProducesResponseType(typeof(List<int>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> GetBookingsToNotify([FromQuery] DateTimeOffset? date)
        {
            if (!date.HasValue)
                return BadRequest(ProblemDetailsBuilder.Build($"Date should be specified"));

            return Ok(await _bookingsProcessingService.GetForNotification(date.Value));
        }


        /// <summary>
        ///     Sends need payment notifications for bookings
        /// </summary>
        /// <param name="bookingIds">List of booking ids for notify</param>
        /// <returns>Result message</returns>
        [HttpPost("bookings/notifications/deadline-approach/send")]
        [ProducesResponseType(typeof(BatchOperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> NotifyPaymentsNeeded(List<int> bookingIds)
        {
            var (_, _, serviceAccount, _) = await _serviceAccountContext.GetCurrent();
            return OkOrBadRequest(await _bookingsProcessingService.NotifyDeadlineApproaching(bookingIds, serviceAccount));
        }


        /// <summary>
        ///     Sends need payment notifications for offline bookings
        /// </summary>
        /// <returns>Result message</returns>
        [HttpPost("bookings/notifications/offline-deadline-approach/send")]
        [ProducesResponseType(typeof(BatchOperationResult), (int)HttpStatusCode.OK)]
        [ServiceAccountRequired]
        public async Task<IActionResult> NotifyOfflinePaymentsNeeded()
        {
            return Ok(await _bookingsProcessingService.NotifyOfflineDeadlineApproaching());
        }


        /// <summary>
        ///     Sends bookings summary reports
        /// </summary>
        /// <returns>Result message</returns>
        [HttpPost("bookings/notifications/agent-summary/send")]
        [ProducesResponseType(typeof(BatchOperationResult), (int)HttpStatusCode.OK)]
        [ServiceAccountRequired]
        public async Task<IActionResult> NotifyBookingSummary()
        {
            return Ok(await _bookingsProcessingService.SendBookingSummaryReports());
        }


        /// <summary>
        ///     Sends bookings summary report for administrator
        /// </summary>
        /// <returns>Result message</returns>
        [HttpPost("bookings/notifications/administrator-summary/send")]
        [ProducesResponseType(typeof(BatchOperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> NotifyBookingsSummaryAdministrator()
        {
            return OkOrBadRequest(await _reportsService.SendBookingsAdministratorSummary());
        }


        /// <summary>
        ///     Sends bookings monthly summary report for administrator
        /// </summary>
        /// <returns>Result message</returns>
        [HttpPost("bookings/notifications/administrator-payment-summary/send")]
        [ProducesResponseType(typeof(BatchOperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> NotifyBookingsPaymentsSummaryAdministrator()
        {
            // TODO: Ad-hoc solution, change to more appropriate
            return OkOrBadRequest(await _reportsService.SendBookingsPaymentsSummaryToAdministrator());
        }


        /// <summary>
        ///     Sends notifications when funds on the agency balance decrease below thresholds
        /// </summary>
        /// <returns>Result message</returns>
        [HttpPost("agencies/notifications/credit-limit-run-out-balance/send")]
        [ProducesResponseType(typeof(BatchOperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> NotifyCreditLimitRunOutBalance()
            => OkOrBadRequest(await _balanceNotificationService.NotifyCreditLimitRunOutBalance());


        /// <summary>
        ///     Get applied markups for materialization
        /// </summary>
        [HttpGet("bookings/markup-bonuses")]
        [ProducesResponseType(typeof(List<int>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> GetAppliedMarkupsForMaterialization([FromQuery] DateTimeOffset? date)
        {
            if (!date.HasValue)
                return BadRequest(ProblemDetailsBuilder.Build($"Date should be specified"));

            return Ok(await _markupBonusMaterializationService.GetForMaterialize(date.Value));
        }


        /// <summary>
        ///     Materializes markup bonuses
        /// </summary>
        [HttpPost("bookings/markup-bonuses")]
        [ProducesResponseType(typeof(BatchOperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> MaterializeBookingsMarkupBonuses(List<int> appliedMarkups)
        {
            return OkOrBadRequest(await _markupBonusMaterializationService.Materialize(appliedMarkups));
        }


        /// <summary>
        ///     Get bookings ids for refreshing status
        /// </summary>
        [HttpGet("bookings/statuses/refresh")]
        [ProducesResponseType(typeof(List<int>), (int)HttpStatusCode.OK)]
        [ServiceAccountRequired]
        public async Task<IActionResult> GetBookingIdsForStatusRefresh()
        {
            return Ok(await _bookingRefreshStatusService.GetBookingsToRefresh());
        }


        /// <summary>
        ///     Refresh booking statuses
        /// </summary>
        [HttpPost("bookings/statuses/refresh")]
        [ProducesResponseType(typeof(BatchOperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> RefreshBookingStatuses(List<int> bookingIds)
        {
            var (_, _, serviceAccount, _) = await _serviceAccountContext.GetCurrent();
            return OkOrBadRequest(await _bookingRefreshStatusService.RefreshStatuses(bookingIds, serviceAccount.ToApiCaller()));
        }

        /// <summary>
        ///     Set booking statuses to Completed
        /// </summary>
        /// <returns>Updated booking Ids</returns>
        [HttpPost("bookings/statuses/complete")]
        [ProducesResponseType(typeof(List<int>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> SetBookingStatusesCompleted()
        {
            return OkOrBadRequest(await _bookingRefreshStatusService.SetBookingStatusesCompleted());
        }


        /// <summary>
        ///     Get payment ids for refund
        /// </summary>
        [HttpGet("bookings/refunds")]
        [ProducesResponseType(typeof(List<int>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> GetPaymentsForRefund([FromQuery] DateTimeOffset? date)
            => Ok(await _refundService.GetPaymentsForRefund(date));


        /// <summary>
        ///     Refunds payments
        /// </summary>
        [HttpPost("bookings/refunds")]
        [ProducesResponseType(typeof(BatchOperationResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ServiceAccountRequired]
        public async Task<IActionResult> RefundPayments(List<int> paymentIds)
            => OkOrBadRequest(await _refundService.RefundPayments(paymentIds));


        private readonly IBookingsProcessingService _bookingsProcessingService;
        private readonly IServiceAccountContext _serviceAccountContext;
        private readonly IBookingReportsService _reportsService;
        private readonly IMarkupBonusMaterializationService _markupBonusMaterializationService;
        private readonly IBookingStatusRefreshService _bookingRefreshStatusService;
        private readonly INGeniusRefundService _refundService;
        private readonly IBalanceNotificationsService _balanceNotificationService;
    }
}