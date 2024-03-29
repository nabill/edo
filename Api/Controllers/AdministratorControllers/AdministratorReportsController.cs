using System;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Filters.Authorization.AdministratorFilters;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Reports;
using HappyTravel.Edo.Common.Enums.Administrators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/reports")]
    [Produces("application/json")]
    public class AdministratorReportsController : BaseController
    {
        public AdministratorReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }


        /// <summary>
        ///     Returns payable to supplier direct connectivity report
        /// </summary>
        [HttpGet("direct-connectivity-report/payable-to-supplier")]
        [ProducesResponseType(typeof(FileStream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingReportGeneration)]
        public async Task<IActionResult> PayableToSupplierDirectConnectivityReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _reportService.GetPayableToSupplierReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"direct-connectivity-report-suppliers-{from:g}-{end:g}.csv"
            };
        }


        /// <summary>
        ///     Returns agency wise direct connectivity report
        /// </summary>
        [HttpGet("direct-connectivity-report/agency-wise")]
        [ProducesResponseType(typeof(FileStream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingReportGeneration)]
        public async Task<IActionResult> GetAgencyWiseDirectConnectivityReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _reportService.GetAgencyWiseReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"direct-connectivity-report-agencies-{from:g}-{end:g}.csv"
            };
        }


        /// <summary>
        ///     Returns agencies productivity report
        /// </summary>
        [HttpGet("agencies-productivity-report")]
        [ProducesResponseType(typeof(FileStream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingReportGeneration)]
        public async Task<IActionResult> GetAgenciesProductivityReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _reportService.AgenciesProductivityReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"agencies-productivity-report-{from:g}-{end:g}.csv"
            };
        }


        /// <summary>
        ///     Returns full bookings report
        /// </summary>
        [HttpGet("full-bookings-report")]
        [ProducesResponseType(typeof(FileStream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingReportGeneration)]
        public async Task<IActionResult> GetFullBookingReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _reportService.GetFullBookingsReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"full-bookings-report-{from:g}-{end:g}.csv"
            };
        }


        /// <summary>
        ///     Returns pending supplier reference report
        /// </summary>
        [HttpGet("pending-supplier-reference-report")]
        [ProducesResponseType(typeof(FileStream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingReportGeneration)]
        public async Task<IActionResult> GetPendingSupplierReferenceReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _reportService.PendingSupplierReferenceReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"pending-supplier-reference-report-{from:g}-{end:g}.csv"
            };
        }


        /// <summary>
        ///     Returns confirmed bookings report
        /// </summary>
        [HttpGet("confirmed-bookings-report")]
        [ProducesResponseType(typeof(FileStream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingReportGeneration)]
        public async Task<IActionResult> GetConfirmedBookingsReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _reportService.ConfirmedBookingsReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"confirmed-bookings-report-{from:g}-{end:g}.csv"
            };
        }

        /// <summary>
        ///     Returns finalized bookings report
        /// </summary>
        [HttpGet("finalized-bookings-report")]
        [ProducesResponseType(typeof(FileStream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingReportGeneration)]
        public async Task<IActionResult> GetFinalizedBookingReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _reportService.GetFinalizedBookingsReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"finalized-bookings-report-{from:g}-{end:g}.csv"
            };
        }


        /// <summary>
        ///     Returns hotel wise booking report 
        /// </summary>
        [HttpGet("hotel-wise-booking-report")]
        [ProducesResponseType(typeof(FileStream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingReportGeneration)]
        public async Task<IActionResult> GetHotelWiseReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _reportService.GetHotelWiseBookingReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"hotel-wise-report-{from:g}-{end:g}.csv"
            };
        }


        /// <summary>
        ///     Returns cancellation deadline report 
        /// </summary>
        [HttpGet("cancellation-deadline-report")]
        [ProducesResponseType(typeof(FileStream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingReportGeneration)]
        public async Task<IActionResult> GetCancellationDeadlineReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _reportService.GetCancellationDeadlineReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"cancellation-deadline-report-{from:g}-{end:g}.csv"
            };
        }


        /// <summary>
        ///     Returns third party suppliers report 
        /// </summary>
        [HttpGet("third-party-suppliers-report")]
        [ProducesResponseType(typeof(FileStream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingReportGeneration)]
        public async Task<IActionResult> GetThirdPartySuppliersReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _reportService.GetThirdPartySuppliersReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"third-party-suppliers-report-{from:g}-{end:g}.csv"
            };
        }


        /// <summary>
        ///     Returns vcc bookings report 
        /// </summary>
        [HttpGet("vcc-bookings-report")]
        [ProducesResponseType(typeof(FileStream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingReportGeneration)]
        public async Task<IActionResult> GetVccBookingReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _reportService.GetVccBookingReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"vcc-bookings-report-{from:g}-{end:g}.csv"
            };
        }


        [HttpGet("hotel-productivity-report")]
        [ProducesResponseType(typeof(FileStream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingReportGeneration)]
        public async Task<IActionResult> GetHotelProductivityReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _reportService.GetHotelProductivityReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"hotel-productivity-report-{from:g}-{end:g}.csv"
            };
        }


        /// <summary>
        ///     Returns cancelled bookings report 
        /// </summary>
        [HttpGet("cancelled-bookings-report")]
        [ProducesResponseType(typeof(FileStream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.BookingReportGeneration)]
        public async Task<IActionResult> GetCancelledBookingsReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _reportService.GetCancelledBookingsReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"cancelled-bookings-report-{from:g}-{end:g}.csv"
            };
        }

        
        /// <summary>
        /// Returns agencies report 
        /// </summary>
        [HttpGet("agencies-report")]
        [ProducesResponseType(typeof(FileStream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.MarketingReportGeneration)]
        public async Task<IActionResult> GetAgenciesReport()
        {
            var (_, isFailure, stream, error) = await _reportService.GetAgenciesReport();
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"agencies-report.csv"
            };
        }
        

        /// <summary>
        ///     Returns payment link report 
        /// </summary>
        [HttpGet("payment-link-report")]
        [ProducesResponseType(typeof(FileStream), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [AdministratorPermissions(AdministratorPermissions.FinanceReportGeneration)]
        public async Task<IActionResult> GetPaymentLinkReport(DateTime from, DateTime end)
        {
            var (_, isFailure, stream, error) = await _reportService.GetPaymentLinkReport(from, end);
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return new FileStreamResult(stream, new MediaTypeHeaderValue("text/csv"))
            {
                FileDownloadName = $"payment-link-report-{from:g}-{end:g}.csv"
            };
        }

        private readonly IReportService _reportService;
    }
}