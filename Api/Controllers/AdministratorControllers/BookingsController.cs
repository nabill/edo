using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNet.OData;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin")]
    [Produces("application/json")]
    public class BookingsController : BaseController
    {
        public BookingsController(IAdministratorContext administratorContext,
            IBookingService bookingService,
            IAdministratorBookingManagementService bookingManagementService,
            IBookingInfoService bookingInfoService,
            IFixHtIdService fixHtIdService)
        {
            _administratorContext = administratorContext;
            _bookingService = bookingService;
            _bookingManagementService = bookingManagementService;
            _bookingInfoService = bookingInfoService;
            _fixHtIdService = fixHtIdService;
        }
        
        
        /// <summary>
        ///     Gets all bookings
        /// </summary>
        /// <returns>List of bookings</returns>
        [HttpGet("accommodations/bookings")]
        [ProducesResponseType(typeof(List<BookingSlim>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingManagement)]
        [EnableQuery(PageSize = 500, MaxTop = 500)]
        public IEnumerable<BookingSlim> GetAgencyBookings() 
            => _bookingService.GetAllBookings();


        /// <summary>
        ///     Gets a list of all bookings made by the agency
        /// </summary>
        /// <param name="agencyId">Agency Id</param>
        /// <returns>List of bookings</returns>
        [HttpGet("agencies/{agencyId}/accommodations/bookings")]
        [ProducesResponseType(typeof(List<BookingSlim>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingManagement)]
        [EnableQuery(PageSize = 500, MaxTop = 500)]
        public IEnumerable<BookingSlim> GetAgencyBookings([FromRoute] int agencyId) 
            => _bookingService.GetAgencyBookings(agencyId);


        /// <summary>
        ///     Gets a list of all bookings made by the agent
        /// </summary>
        /// <param name="agentId">Agent Id</param>
        /// <returns>List of bookings</returns>
        [HttpGet("agents/{agentId}/accommodations/bookings")]
        [ProducesResponseType(typeof(List<BookingSlim>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingManagement)]
        [EnableQuery(PageSize = 500, MaxTop = 500)]
        public IEnumerable<BookingSlim> GetAgentBookings([FromRoute] int agentId) 
            => _bookingService.GetAgentBookings(agentId);


        /// <summary>
        ///     Gets booking data by reference code.
        /// </summary>
        /// <param name="referenceCode">Booking reference code</param>
        /// <returns>Booking Info</returns>
        [HttpGet("bookings/{referenceCode}")]
        [ProducesResponseType(typeof(AccommodationBookingInfo), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingManagement)]
        public async Task<IActionResult> GetBookingByReferenceCode(string referenceCode)
        {
            var (_, isFailure, bookingData, error) =
                await _bookingInfoService.GetAccommodationBookingInfo(referenceCode, LanguageCode);

            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(bookingData);
        }


        /// <summary>
        ///     Gets booking confirmation changes history
        /// </summary>
        /// <param name="referenceCode">Booking reference code for retrieving confirmation change history</param>
        /// <returns>List of booking confirmation change events</returns>
        [HttpGet("accommodations/bookings/{referenceCode}/confirmation-history")]
        [ProducesResponseType(typeof(List<BookingConfirmationHistoryEntry>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingManagement)]
        public async Task<IActionResult> GetBookingConfirmationCodeHistory([FromRoute] string referenceCode)
        {
            return OkOrBadRequest(await _bookingInfoService.GetBookingConfirmationHistory(referenceCode));
        }


        /// <summary>
        ///     Discards accommodation booking by admin.
        /// </summary>
        /// <param name="bookingId">Id of booking to cancel</param>
        /// <returns></returns>
        [HttpPost("accommodations/bookings/{bookingId}/discard")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingManagement)]
        public async Task<IActionResult> Discard(int bookingId)
        {
            var (_, _, admin, _) = await _administratorContext.GetCurrent();
            var (_, isFailure, error) = await _bookingManagementService.Discard(bookingId, admin);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Refreshes accommodation booking by admin.
        /// </summary>
        /// <param name="bookingId">Id of booking to cancel</param>
        /// <returns></returns>
        [HttpPost("accommodations/bookings/{bookingId}/refresh-status")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingManagement)]
        public async Task<IActionResult> Refresh(int bookingId)
        {
            var (_, _, admin, _) = await _administratorContext.GetCurrent();
            var (_, isFailure, error) = await _bookingManagementService.RefreshStatus(bookingId, admin);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Cancel accommodation booking by admin.
        /// </summary>
        /// <param name="bookingId">Id of booking to cancel</param>
        /// <returns></returns>
        [HttpPost("accommodations/bookings/{bookingId}/cancel")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingManagement)]
        public async Task<IActionResult> Cancel(int bookingId)
        {
            var (_, _, admin, _) = await _administratorContext.GetCurrent();
            var (_, isFailure, error) = await _bookingManagementService.Cancel(bookingId, admin);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Cancel accommodation booking manually, without requests to supplier. Cancellation penalties are applied.
        /// </summary>
        /// <param name="bookingId">Id of booking to cancel</param>
        /// <param name="cancellationRequest">Cancellation request</param>
        /// <returns></returns>
        [HttpPost("accommodations/bookings/{bookingId}/cancel-manually")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingManagement)]
        public async Task<IActionResult> CancelManually(int bookingId, [FromBody] ManualBookingCancellationRequest cancellationRequest)
        {
            var (_, _, admin, _) = await _administratorContext.GetCurrent();
            var (_, isFailure, error) =
                await _bookingManagementService.CancelManually(bookingId, cancellationRequest.CancellationDate, cancellationRequest.Reason, admin);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Reject accommodation booking manually, without requests to supplier. Cancellation penalties not applied.
        /// </summary>
        /// <param name="bookingId">Id of booking to reject</param>
        /// <param name="rejectionRequest">Rejection request</param>
        /// <returns></returns>
        [HttpPost("accommodations/bookings/{bookingId}/reject-manually")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingManagement)]
        public async Task<IActionResult> Reject(int bookingId, [FromBody] ManualBookingRejectionRequest rejectionRequest)
        {
            var (_, _, admin, _) = await _administratorContext.GetCurrent();
            var (_, isFailure, error) = await _bookingManagementService.RejectManually(bookingId, rejectionRequest.Reason, admin);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Confirm accommodation booking manually
        /// </summary>
        /// <param name="bookingId">Id of booking to confirm</param>
        /// <param name="confirmationRequest">Confirmation request</param>
        /// <returns></returns>
        [HttpPost("accommodations/bookings/{bookingId}/confirm-manually")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingManagement)]
        public async Task<IActionResult> ConfirmManually(int bookingId, [FromBody] ManualBookingConfirmationRequest confirmationRequest)
        {
            var (_, _, admin, _) = await _administratorContext.GetCurrent();
            var (_, isFailure, error) =
                await _bookingManagementService.ConfirmManually(bookingId, confirmationRequest.ConfirmationDate, confirmationRequest.Reason, admin);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }
        
        
        /// <summary>
        ///     Fills empty htId in bookings
        /// </summary>
        /// <returns></returns>
        [HttpPost("accommodations/bookings/fill-empty-htids")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingManagement)]
        public async Task<IActionResult> FillEmptyHtIds()
        {
            await _fixHtIdService.FillEmptyHtIds();
            return NoContent();
        }
        
        
        /// <summary>
        ///     Gets booking status changes history
        /// </summary>
        /// <param name="bookingId">Booking ID for retrieving status change history</param>
        /// <returns>List of booking status change events</returns>
        [HttpGet("accommodations/bookings/{bookingId}/status-history")]
        [ProducesResponseType(typeof(List<BookingStatusHistoryEntry>), StatusCodes.Status200OK)]
        [AdministratorPermissions(AdministratorPermissions.BookingManagement)]
        public async Task<ActionResult<List<BookingStatusHistoryEntry>>> GetBookingStatusHistory(int bookingId) 
            => await _bookingInfoService.GetBookingStatusHistory(bookingId);


        private readonly IAdministratorContext _administratorContext;
        private readonly IBookingService _bookingService;
        private readonly IAdministratorBookingManagementService _bookingManagementService;
        private readonly IBookingInfoService _bookingInfoService;
        private readonly IFixHtIdService _fixHtIdService;
    }
}