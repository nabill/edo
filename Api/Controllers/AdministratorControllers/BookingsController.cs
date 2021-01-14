using System.Net;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Services.Management;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/accommodations/bookings")]
    [Produces("application/json")]
    public class BookingsController : BaseController
    {
        public BookingsController(IAdministratorContext administratorContext,
            IAdministratorBookingManagementService bookingManagementService)
        {
            _administratorContext = administratorContext;
            _bookingManagementService = bookingManagementService;
        }
        
        
        /// <summary>
        ///     Cancels accommodation booking by admin.
        /// </summary>
        /// <param name="bookingId">Id of booking to cancel</param>
        /// <returns></returns>
        [HttpPost("{bookingId}/discard")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingManagement)]
        public async Task<IActionResult> Discard(int bookingId)
        {
            var (_, _, admin, _) = await _administratorContext.GetCurrent();
            var (_, isFailure, error) = await _bookingManagementService.Discard(bookingId, admin);
            if (isFailure)
                return BadRequest(error);

            return NoContent();
        } 
        
        
        /// <summary>
        ///     Cancel accommodation booking by admin.
        /// </summary>
        /// <param name="bookingId">Id of booking to cancel</param>
        /// <param name="requireSupplierConfirmation">If a supplier returns an error after cancellation request, this is ignored as if it was a success</param>
        /// <returns></returns>
        [HttpPost("{bookingId}/cancel")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingManagement)]
        public async Task<IActionResult> Cancel(int bookingId, [FromQuery] bool requireSupplierConfirmation = true)
        {
            var (_, _, admin, _) = await _administratorContext.GetCurrent();
            var (_, isFailure, error) = await _bookingManagementService.Cancel(bookingId, admin, requireSupplierConfirmation);
            if (isFailure)
                return BadRequest(error);

            return NoContent();
        }
        
        
        private readonly IAdministratorContext _administratorContext;
        private readonly IAdministratorBookingManagementService _bookingManagementService;
    }
}