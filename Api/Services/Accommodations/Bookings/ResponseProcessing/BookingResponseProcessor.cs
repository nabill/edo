using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing
{
    public class BookingResponseProcessor : IBookingResponseProcessor
    {
        public BookingResponseProcessor(IBookingAuditLogService bookingAuditLogService,
            IBookingRecordManager bookingRecordManager,
            IBookingChangesProcessor bookingChangesProcessor,
            ILogger<BookingResponseProcessor> logger)
        {
            _bookingAuditLogService = bookingAuditLogService;
            _bookingRecordManager = bookingRecordManager;
            _bookingChangesProcessor = bookingChangesProcessor;
            _logger = logger;
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
                await _bookingChangesProcessor.ProcessBookingNotFound(booking, bookingResponse);
                return;
            }

            if (bookingResponse.Status.ToInternalStatus() == booking.Status)
            {
                _logger.LogBookingResponseProcessSuccess(
                    $"The booking response with the reference code '{bookingResponse.ReferenceCode}' has been successfully processed. No changes applied");
                return;
            }

            await UpdateBookingDetails();

            switch (bookingResponse.Status)
            {
                case BookingStatusCodes.Confirmed:
                    await _bookingChangesProcessor.ProcessConfirmation(booking, bookingResponse);
                    break;
                case BookingStatusCodes.Cancelled:
                    await _bookingChangesProcessor.ProcessCancellation(booking, UserInfo.InternalServiceAccount);
                    break;
                case BookingStatusCodes.Rejected:
                    await _bookingChangesProcessor.ProcessRejection(booking, UserInfo.InternalServiceAccount);
                    break;
            }

            _logger.LogBookingResponseProcessSuccess(
                $"The booking response with the reference code '{bookingResponse.ReferenceCode}' has been successfully processed. " +
                $"New status: {bookingResponse.Status}");


            Task UpdateBookingDetails() => _bookingRecordManager.UpdateBookingDetails(bookingResponse, booking);
            
            //TICKET https://happytravel.atlassian.net/browse/NIJO-315
            /*
            async Task<Result> LogAppliedMarkups()
            {
                long availabilityId = ??? ;
                
                var (_, isGetAvailabilityFailure, responseWithMarkup, cachedAvailabilityError) = await _availabilityResultsCache.Get(availabilityId);
                if (isGetAvailabilityFailure)
                    return Result.Fail(cachedAvailabilityError);

                await _markupLogger.Write(bookingResponse.ReferenceCode, ServiceTypes.HTL, responseWithMarkup.AppliedPolicies);
                return Result.Success();
            }
            */
        }
        
        private readonly IBookingAuditLogService _bookingAuditLogService;
        private readonly IBookingRecordManager _bookingRecordManager;
        private readonly IBookingChangesProcessor _bookingChangesProcessor;
        private readonly ILogger<BookingResponseProcessor> _logger;
    }
}