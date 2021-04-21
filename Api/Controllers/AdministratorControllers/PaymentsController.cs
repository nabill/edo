using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Management.Enums;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Microsoft.AspNetCore.Mvc;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices;
using HappyTravel.Edo.Api.Models.Management;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/payments")]
    [Produces("application/json")]
    public class PaymentsController : BaseController
    {
        public PaymentsController(IAdministratorContext administratorContext, ICounterpartyAccountService counterpartyAccountService,
            IAgencyAccountService agencyAccountService,
            ICreditCardPaymentConfirmationService creditCardPaymentConfirmationService,
            IBookingOfflinePaymentService offlinePaymentService)
        {
            _administratorContext = administratorContext;
            _counterpartyAccountService = counterpartyAccountService;
            _agencyAccountService = agencyAccountService;
            _creditCardPaymentConfirmationService = creditCardPaymentConfirmationService;
            _offlinePaymentService = offlinePaymentService;
        }


        /// <summary>
        ///     Completes payment manually
        /// </summary>
        /// <param name="bookingId">Booking id for completion</param>
        [HttpPost("offline/{bookingId}")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.OfflinePayment)]
        public async Task<IActionResult> CompleteOffline(int bookingId)
        {
            var (_, _, administrator, _) = await _administratorContext.GetCurrent();
            var (_, isFailure, error) = await _offlinePaymentService.CompleteOffline(bookingId, administrator);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return NoContent();
        }


        /// <summary>
        ///     Confirm credit card payment
        /// </summary>
        [HttpPost("credit-card/{bookingId}/confirm")]
        [ProducesResponseType((int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), (int) HttpStatusCode.BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.OfflinePayment)]
        public async Task<IActionResult> ConfirmCreditCartPayment(int bookingId)
        {
            var (isSuccess, _, error) = await _creditCardPaymentConfirmationService.Confirm(bookingId);

            return isSuccess
                ? NoContent()
                : (IActionResult) BadRequest(ProblemDetailsBuilder.Build(error));
        }


      


        private readonly IAgencyAccountService _agencyAccountService;
        private readonly IAdministratorContext _administratorContext;
        private readonly ICounterpartyAccountService _counterpartyAccountService;
        private readonly ICreditCardPaymentConfirmationService _creditCardPaymentConfirmationService;
        private readonly IBookingOfflinePaymentService _offlinePaymentService;
    }
}