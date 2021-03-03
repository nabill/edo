using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing
{
    public class BookingResponseProcessor : IBookingResponseProcessor
    {
        public BookingResponseProcessor(IBookingAuditLogService bookingAuditLogService,
            IBookingRecordManager bookingRecordManager,
            ILogger<BookingResponseProcessor> logger,
            IDateTimeProvider dateTimeProvider, 
            IBookingStatusChangesProcessor statusChangesProcessor)
        {
            _bookingAuditLogService = bookingAuditLogService;
            _bookingRecordManager = bookingRecordManager;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _statusChangesProcessor = statusChangesProcessor;
        }
        
        
        public async Task ProcessResponse(Booking bookingResponse)
        {
            var (_, isFailure, booking, error) = await _bookingRecordManager.Get(bookingResponse.ReferenceCode);
            if (isFailure)
            {
                _logger.LogBookingResponseProcessFailure(error);
                return;
            }
            
            _logger.LogBookingResponseProcessStarted(
                $"Start the booking response processing with the reference code '{bookingResponse.ReferenceCode}'. Old status: {booking.Status}");

            
            await _bookingAuditLogService.Add(bookingResponse, booking);

            if (bookingResponse.Status == BookingStatusCodes.NotFound)
            {
                await ProcessBookingNotFound(booking, bookingResponse);
                return;
            }

            if (bookingResponse.Status.ToInternalStatus() == booking.Status)
            {
                _logger.LogBookingResponseProcessSuccess(
                    $"The booking response with the reference code '{bookingResponse.ReferenceCode}' has been successfully processed. No changes applied");
                return;
            }

            await _bookingRecordManager.UpdateBookingFromDetails(bookingResponse, booking);

            switch (bookingResponse.Status)
            {
                case BookingStatusCodes.Confirmed:
                    await _statusChangesProcessor.ProcessConfirmation(booking);
                    break;
                case BookingStatusCodes.Cancelled:
                    await _statusChangesProcessor.ProcessCancellation(booking, _dateTimeProvider.UtcNow(), UserInfo.InternalServiceAccount);
                    break;
                case BookingStatusCodes.Rejected:
                    await _statusChangesProcessor.ProcessRejection(booking, UserInfo.InternalServiceAccount);
                    break;
            }

            _logger.LogBookingResponseProcessSuccess(
                $"The booking response with the reference code '{bookingResponse.ReferenceCode}' has been successfully processed. " +
                $"New status: {bookingResponse.Status}");
        }
        
        
        private async Task ProcessBookingNotFound(Edo.Data.Bookings.Booking booking, EdoContracts.Accommodations.Booking bookingResponse)
        {
            if (_dateTimeProvider.UtcNow() < booking.Created + BookingCheckTimeout)
            {
                _logger.LogBookingResponseProcessSuccess(
                    $"The booking response with the reference code '{bookingResponse.ReferenceCode}' has not been processed due to '{BookingStatusCodes.NotFound}' status.");
            }
            else
            {
                await _bookingRecordManager.SetStatus(booking.ReferenceCode, BookingStatuses.ManualCorrectionNeeded);
                _logger.LogBookingResponseProcessSuccess(
                    $"The booking response with the reference code '{bookingResponse.ReferenceCode}' set as needed manual processing.");
            }
        }
        
        
        private static readonly TimeSpan BookingCheckTimeout = TimeSpan.FromMinutes(30);
        
        private readonly IBookingAuditLogService _bookingAuditLogService;
        private readonly ILogger<BookingResponseProcessor> _logger;
        private readonly IBookingRecordManager _bookingRecordManager;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingStatusChangesProcessor _statusChangesProcessor;
    }
}