using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing
{
    public class BookingResponseProcessor : IBookingResponseProcessor
    {
        public BookingResponseProcessor(IBookingAuditLogService bookingAuditLogService,
            IBookingRecordManager bookingRecordManager,
            ILogger<BookingResponseProcessor> logger,
            ISupplierOrderService supplierOrderService,
            IBookingNotificationService bookingNotificationService,
            IDateTimeProvider dateTimeProvider, 
            IBookingMoneyReturnService moneyReturnService,
            IBookingInfoService bookingInfoService,
            IBookingDocumentsMailingService documentsMailingService,
            EdoContext context)
        {
            _bookingAuditLogService = bookingAuditLogService;
            _bookingRecordManager = bookingRecordManager;
            _logger = logger;
            _supplierOrderService = supplierOrderService;
            _bookingNotificationService = bookingNotificationService;
            _dateTimeProvider = dateTimeProvider;
            _moneyReturnService = moneyReturnService;
            _bookingInfoService = bookingInfoService;
            _documentsMailingService = documentsMailingService;
            _context = context;
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

            await UpdateBookingDetails();

            switch (bookingResponse.Status)
            {
                case BookingStatusCodes.Confirmed:
                    await ProcessConfirmation(booking, bookingResponse);
                    break;
                case BookingStatusCodes.Cancelled:
                    await ProcessCancellation(booking, UserInfo.InternalServiceAccount);
                    break;
                case BookingStatusCodes.Rejected:
                    await ProcessRejection(booking, UserInfo.InternalServiceAccount);
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
        
        private Task<Result> ProcessCancellation(Edo.Data.Bookings.Booking booking, UserInfo user)
        {
            return SendNotifications()
                .Tap(CancelSupplierOrder)
                .Bind(() => ReturnMoney(booking, user))
                .Tap(SetBookingCancelled);

            
            Task CancelSupplierOrder() 
                => _supplierOrderService.Cancel(booking.ReferenceCode);


            async Task<Result> SendNotifications()
            {
                var agent = await _context.Agents.SingleOrDefaultAsync(a => a.Id == booking.AgentId);
                if (agent == default)
                {
                    _logger.LogWarning("Booking cancellation notification: could not find agent with id '{0}' for the booking '{1}'",
                        booking.AgentId, booking.ReferenceCode);

                    return Result.Success();
                }

                var (_, _, bookingInfo, _) = await _bookingInfoService.GetAccommodationBookingInfo(booking.ReferenceCode, booking.LanguageCode);
                await _bookingNotificationService.NotifyBookingCancelled(bookingInfo);
                
                return Result.Success();
            }


            Task SetBookingCancelled() 
                => _bookingRecordManager.SetStatus(booking.ReferenceCode, BookingStatuses.Cancelled);
        }
        
        
        private async Task ProcessConfirmation(Edo.Data.Bookings.Booking booking, EdoContracts.Accommodations.Booking bookingResponse)
        {
            await GetBookingInfo(booking.ReferenceCode, booking.LanguageCode)
                .Tap(Confirm)
                .Tap(NotifyBookingFinalization)
                .Bind(SendInvoice)
                .OnFailure(WriteFailureLog);
            
            
            Task<Result<AccommodationBookingInfo>> GetBookingInfo(string referenceCode, string languageCode) 
                => _bookingInfoService.GetAccommodationBookingInfo(referenceCode, languageCode);


            Task Confirm(AccommodationBookingInfo bookingInfo) 
                => _bookingRecordManager.Confirm(bookingResponse, booking);
            
            
            Task NotifyBookingFinalization(AccommodationBookingInfo bookingInfo) 
                => _bookingNotificationService.NotifyBookingFinalized(bookingInfo);


            Task<Result> SendInvoice(AccommodationBookingInfo bookingInfo) 
                => _documentsMailingService.SendInvoice(bookingInfo.BookingId, bookingInfo.AgentInformation.AgentEmail, booking.AgentId, true);


            void WriteFailureLog(string error) 
                => _logger.LogBookingConfirmationFailure($"Booking '{booking.ReferenceCode} confirmation failed: '{error}");
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


        private Task ProcessRejection(Edo.Data.Bookings.Booking booking, UserInfo user)
        {
            return CancelSupplierOrder()
                .Bind(() => ReturnMoney(booking, user))
                .Tap(SetBookingRejected);

            
            async Task<Result> CancelSupplierOrder()
            {
                await _supplierOrderService.Cancel(booking.ReferenceCode);
                return Result.Success();
            }


            Task SetBookingRejected() 
                => _bookingRecordManager.SetStatus(booking.ReferenceCode, BookingStatuses.Rejected);
        }


        Task<Result> ReturnMoney(Edo.Data.Bookings.Booking booking, UserInfo user) 
            => _moneyReturnService.ReturnMoney(booking, user);
        
        
        private static readonly TimeSpan BookingCheckTimeout = TimeSpan.FromMinutes(30);
        
        private readonly IBookingAuditLogService _bookingAuditLogService;
        private readonly ILogger<BookingResponseProcessor> _logger;
        private readonly ISupplierOrderService _supplierOrderService;
        private readonly IBookingRecordManager _bookingRecordManager;
        private readonly IBookingNotificationService _bookingNotificationService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingMoneyReturnService _moneyReturnService;
        private readonly IBookingInfoService _bookingInfoService;
        private readonly IBookingDocumentsMailingService _documentsMailingService;
        private readonly EdoContext _context;
    }
}