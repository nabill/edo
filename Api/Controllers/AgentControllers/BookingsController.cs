using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AgentExistingFilters;
using HappyTravel.Edo.Api.Filters.Authorization.AgencyVerificationStatesFilters;
using HappyTravel.Edo.Api.Filters.Authorization.InAgencyPermissionFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Locking;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution.Flows;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Money.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using BookingStatusHistoryEntry = HappyTravel.Edo.Data.Bookings.BookingStatusHistoryEntry;
using Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Controllers.AgentControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/accommodations/bookings")]
    [Produces("application/json")]
    public class BookingsController : BaseController
    {
        public BookingsController(IFinancialAccountBookingFlow financialAccountBookingFlow,
            IBankCreditCardBookingFlow bankCreditCardBookingFlow,
            IOfflinePaymentBookingFlow offlinePaymentBookingFlow,
            IAgentContextService agentContextService,
            IAgentBookingManagementService bookingManagementService,
            IBookingRecordManager bookingRecordManager,
            IBookingCreditCardPaymentService creditCardPaymentService,
            IBookingInfoService bookingInfoService,
            IDateTimeProvider dateTimeProvider,
            IdempotentBookingExecutor idempotentBookingExecutor)
        {
            _financialAccountBookingFlow = financialAccountBookingFlow;
            _bankCreditCardBookingFlow = bankCreditCardBookingFlow;
            _offlinePaymentBookingFlow = offlinePaymentBookingFlow;
            _agentContextService = agentContextService;
            _bookingManagementService = bookingManagementService;
            _bookingRecordManager = bookingRecordManager;
            _creditCardPaymentService = creditCardPaymentService;
            _bookingInfoService = bookingInfoService;
            _dateTimeProvider = dateTimeProvider;
            _idempotentBookingExecutor = idempotentBookingExecutor;
        }


        /// <summary>
        ///     Initiates the booking procedure. Creates an empty booking record.
        ///     Must be used before a payment request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationBooking)]
        public async Task<IActionResult> RegisterBooking([FromBody] AccommodationBookingRequest request)
            => OkOrBadRequest(await _bankCreditCardBookingFlow.Register(request, await _agentContextService.GetAgent(), LanguageCode));


        /// <summary>
        ///     Creates booking in one step. An account will be used for payment.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("book-by-account")]
        [ProducesResponseType(typeof(AccommodationBookingInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationBooking)]
        public async Task<IActionResult> Book([FromBody] AccommodationBookingRequest request)
        {
            var agentContext = await _agentContextService.GetAgent();
            return OkOrBadRequest(await _idempotentBookingExecutor.Execute(request: request,
                bookingFunction: () => _financialAccountBookingFlow.BookByAccount(request, agentContext, LanguageCode, ClientIp),
                languageCode: LanguageCode));
        }


        /// <summary>
        ///     Creates booking in one step. Must be payed offline before deadline.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("book-for-offline")]
        [ProducesResponseType(typeof(AccommodationBookingInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationBooking)]
        public async Task<IActionResult> BookByOffline([FromBody] AccommodationBookingRequest request)
        {
            var agentContext = await _agentContextService.GetAgent();
            return OkOrBadRequest(await _idempotentBookingExecutor.Execute(request: request,
                bookingFunction: () => _offlinePaymentBookingFlow.Book(request, agentContext, LanguageCode, ClientIp),
                languageCode: LanguageCode));
        }


        /// <summary>
        ///     Sends booking request to supplier and finalize the booking procedure.
        ///     Must be used after a successful payment request.
        /// </summary>
        /// <param name="referenceCode"></param>
        /// <returns></returns>
        [HttpPost("{referenceCode}/finalize")]
        [ProducesResponseType(typeof(AccommodationBookingInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationBooking)]
        public async Task<IActionResult> FinalizeBooking([FromRoute] string referenceCode)
        {
            var agent = await _agentContextService.GetAgent();
            return OkOrBadRequest(await _bankCreditCardBookingFlow.Finalize(referenceCode, agent, LanguageCode));
        }


        /// <summary>
        ///     Sends booking request to a supplier to get refreshed booking details, especially - status.
        /// </summary>
        /// <param name="bookingId">Id of the booking</param>
        /// <returns>Updated booking details.</returns>
        [HttpPost("{bookingId}/refresh-status")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationBooking)]
        public async Task<IActionResult> RefreshStatus([FromRoute] int bookingId)
        {
            var agent = await _agentContextService.GetAgent();
            return OkOrBadRequest(await _bookingManagementService.RefreshStatus(bookingId, agent));
        }


        /// <summary>
        ///     Cancel accommodation booking.
        /// </summary>
        /// <param name="bookingId">Id of booking to cancel</param>
        /// <returns></returns>
        [HttpPost("{bookingId}/cancel")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationBooking)]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var agent = await _agentContextService.GetAgent();
            return NoContentOrBadRequest(await _bookingManagementService.Cancel(bookingId, agent));
        }


        /// <summary>
        ///     Cancel accommodation booking by reference code.
        /// </summary>
        /// <param name="referenceCode">Reference code of booking to cancel</param>
        /// <returns></returns>
        [HttpPost("refcode/{referenceCode}/cancel")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationBooking)]
        public async Task<IActionResult> CancelBookingByReferenceCode(string referenceCode)
        {
            var agent = await _agentContextService.GetAgent();
            return NoContentOrBadRequest(await _bookingManagementService.Cancel(referenceCode, agent));
        }


        /// <summary>
        ///     Gets booking data by a booking Id.
        /// </summary>
        /// <returns>Full booking data.</returns>
        [HttpGet("{bookingId}")]
        [ProducesResponseType(typeof(AccommodationBookingInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        [AgentRequired]
        public async Task<IActionResult> GetBookingById(int bookingId)
            => OkOrBadRequest(await _bookingInfoService.GetAgentAccommodationBookingInfo(bookingId, await _agentContextService.GetAgent(), LanguageCode));


        /// <summary>
        ///     Gets booking data by reference code.
        /// </summary>
        /// <returns>Full booking data.</returns>
        [HttpGet("refcode/{referenceCode}")]
        [ProducesResponseType(typeof(AccommodationBookingInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        [AgentRequired]
        public async Task<IActionResult> GetBookingByReferenceCode(string referenceCode)
            => OkOrBadRequest(await _bookingInfoService.GetAgentAccommodationBookingInfo(referenceCode, await _agentContextService.GetAgent(), LanguageCode));


        /// <summary>
        ///     Pays for account booking using credit card
        /// </summary>
        [HttpPost("refcode/{referenceCode}/pay-with-credit-card")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        [AgentRequired]
        public async Task<IActionResult> PayWithCreditCard(string referenceCode)
            => NoContentOrBadRequest(await _creditCardPaymentService.PayForAccountBooking(referenceCode, await _agentContextService.GetAgent()));


        /// <summary>
        ///     Gets cancellation penalty for cancelling booking
        /// </summary>
        /// <returns>Amount of penalty</returns>
        [HttpGet("{bookingId}/cancellation-penalty")]
        [ProducesResponseType(typeof(MoneyAmount), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        [AgentRequired]
        public async Task<IActionResult> GetBookingCancellationPenalty(int bookingId)
        {
            var agent = await _agentContextService.GetAgent();
            var (_, isFailure, booking, error) = await _bookingRecordManager.Get(bookingId)
                .CheckPermissions(agent);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(BookingCancellationPenaltyCalculator.Calculate(booking, _dateTimeProvider.UtcNow()));
        }


        /// <summary>
        ///     Gets all bookings for a current agent.
        /// </summary>
        /// <returns>List of slim booking data.</returns>
        [ProducesResponseType(typeof(List<SlimAccommodationBookingInfo>), (int)HttpStatusCode.OK)]
        [HttpGet("agent")]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        [AgentRequired]
        [EnableQuery]
        public async Task<ActionResult<IQueryable<SlimAccommodationBookingInfo>>> GetAgentBookings()
            => Ok(_bookingInfoService.GetAgentBookingsInfo(await _agentContextService.GetAgent()));


        /// <summary>
        ///     Gets all bookings for an agency of current agent.
        /// </summary>
        /// <returns>List of slim booking data.</returns>
        [ProducesResponseType(typeof(List<AgentBoundedData<SlimAccommodationBookingInfo>>), (int)HttpStatusCode.OK)]
        [HttpGet("agency")]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        [InAgencyPermissions(InAgencyPermissions.AgencyBookingsManagement)]
        [EnableQuery]
        public async Task<ActionResult<IQueryable<AgentBoundedData<SlimAccommodationBookingInfo>>>> GetAgencyBookings()
            => Ok(_bookingInfoService.GetAgencyBookingsInfo(await _agentContextService.GetAgent()));


        /// <summary>
        ///     Gets booking status changes history
        /// </summary>
        /// <param name="bookingId">Booking ID for retrieving status change history</param>
        /// <returns>List of booking status change events</returns>
        [HttpGet("{bookingId}/status-history")]
        [ProducesResponseType(typeof(List<BookingStatusHistoryEntry>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationBooking)]
        [AgentRequired]
        public async Task<IActionResult> GetBookingStatusHistory(int bookingId)
        {
            var agent = await _agentContextService.GetAgent();

            var (_, bookingIsFailure, _, bookingError) = await _bookingRecordManager.Get(bookingId)
                .CheckPermissions(agent);
            if (bookingIsFailure)
                return BadRequest(ProblemDetailsBuilder.Build(bookingError));

            return Ok(await _bookingInfoService.GetBookingStatusHistory(bookingId));
        }


        /// <summary>
        ///     Gets booking confirmation changes history
        /// </summary>
        /// <param name="referenceCode">Booking reference code for retrieving confirmation change history</param>
        /// <returns>List of booking confirmation change events</returns>
        [HttpGet("reference-code/{referenceCode}/confirmation-history")]
        [ProducesResponseType(typeof(List<BookingConfirmationHistoryEntry>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationBooking)]
        [AgentRequired]
        public async Task<IActionResult> GetBookingConfirmationCodeHistory([FromRoute] string referenceCode)
            => OkOrBadRequest(await _bookingInfoService.GetBookingConfirmationHistory(referenceCode));


        /// <summary>
        ///     Recalculate booking's price after changing payment type
        /// </summary>
        /// <param name="referenceCode"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{referenceCode}/price/recalculate")]
        [ProducesResponseType(typeof(AccommodationBookingInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [MinAgencyVerificationState(AgencyVerificationStates.FullAccess)]
        [InAgencyPermissions(InAgencyPermissions.AccommodationBooking)]
        public async Task<IActionResult> RecalculatePrice([FromRoute] string referenceCode, [FromBody] BookingRecalculatePriceRequest request)
        {
            var agent = await _agentContextService.GetAgent();
            return OkOrBadRequest(await _bookingManagementService.RecalculatePrice(referenceCode, request, agent, LanguageCode));
        }


        private readonly IFinancialAccountBookingFlow _financialAccountBookingFlow;
        private readonly IBankCreditCardBookingFlow _bankCreditCardBookingFlow;
        private readonly IOfflinePaymentBookingFlow _offlinePaymentBookingFlow;
        private readonly IAgentContextService _agentContextService;
        private readonly IAgentBookingManagementService _bookingManagementService;
        private readonly IBookingRecordManager _bookingRecordManager;
        private readonly IBookingCreditCardPaymentService _creditCardPaymentService;
        private readonly IBookingInfoService _bookingInfoService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IdempotentBookingExecutor _idempotentBookingExecutor;
    }
}