using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.Mailing;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing
{
    public class BookingChangesProcessor : IBookingChangesProcessor
    {
        public BookingChangesProcessor(ISupplierOrderService supplierOrderService,
            IBookingRecordsManager bookingRecordsManager,
            IBookingMailingService bookingMailingService,
            ILogger<BookingChangesProcessor> logger,
            IDateTimeProvider dateTimeProvider,
            IBookingDocumentsService documentsService,
            IBookingCreditCardPaymentService creditCardPaymentService,
            IBookingAccountPaymentService accountPaymentService,
            EdoContext context)
        {
            _supplierOrderService = supplierOrderService;
            _bookingRecordsManager = bookingRecordsManager;
            _bookingMailingService = bookingMailingService;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _documentsService = documentsService;
            _creditCardPaymentService = creditCardPaymentService;
            _accountPaymentService = accountPaymentService;
            _context = context;
        }
        
        public Task<Result> ProcessCancellation(Booking booking, UserInfo user)
        {
            return SendNotifications()
                .Tap(CancelSupplierOrder)
                .Bind(ReturnMoney)
                .Tap(SetBookingCancelled);

            
            Task CancelSupplierOrder() => _supplierOrderService.Cancel(booking.ReferenceCode);


            async Task<Result> SendNotifications()
            {
                var agent = await _context.Agents.SingleOrDefaultAsync(a => a.Id == booking.AgentId);
                if (agent == default)
                {
                    _logger.LogWarning("Booking cancellation notification: could not find agent with id '{0}' for the booking '{1}'",
                        booking.AgentId, booking.ReferenceCode);

                    return Result.Success();
                }

                var (_, _, bookingInfo, _) = await _bookingRecordsManager.GetAccommodationBookingInfo(booking.ReferenceCode, booking.LanguageCode);
                await _bookingMailingService.NotifyBookingCancelled(bookingInfo);
                
                return Result.Success();
            }

            async Task<Result> ReturnMoney()
            {
                switch (booking.PaymentMethod)
                {
                    case PaymentMethods.BankTransfer:
                        switch (booking.PaymentStatus)
                        {
                            case BookingPaymentStatuses.NotPaid:
                            case BookingPaymentStatuses.Refunded:
                                break;
                            case BookingPaymentStatuses.Captured:
                                return await _accountPaymentService.Refund(booking, user);;
                            default:
                                throw new ArgumentOutOfRangeException($"Invalid payment status: {booking.PaymentStatus}");
                        }
                        break;
                    
                    case PaymentMethods.CreditCard:
                        switch (booking.PaymentStatus)
                        {
                            case BookingPaymentStatuses.Refunded:
                            case BookingPaymentStatuses.Voided:
                                break;
                            case BookingPaymentStatuses.Authorized:
                                return await _creditCardPaymentService.Void(booking, user);
                            case BookingPaymentStatuses.Captured:
                                return await _creditCardPaymentService.Refund(booking, user);
                            default:
                                throw new ArgumentOutOfRangeException($"Invalid payment status: {booking.PaymentStatus}");
                        }
                        break;
                    
                    case PaymentMethods.Offline:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Invalid payment method: {booking.PaymentMethod}");
                }

                return Result.Success();
            }


            Task SetBookingCancelled() => _bookingRecordsManager.SetStatus(booking, BookingStatuses.Cancelled);
        }
        
        
        public async Task ProcessConfirmation(Booking booking, EdoContracts.Accommodations.Booking bookingResponse)
        {
            await GetBookingInfo(booking.ReferenceCode, booking.LanguageCode)
                .Tap(Confirm)
                .Tap(NotifyBookingFinalization)
                .Bind(GenerateInvoice)
                .Bind(SendInvoice)
                .OnFailure(WriteFailureLog);
            
            Task<Result<AccommodationBookingInfo>> GetBookingInfo(string referenceCode, string languageCode) => _bookingRecordsManager
                .GetAccommodationBookingInfo(referenceCode, languageCode);


            Task Confirm(AccommodationBookingInfo bookingInfo) => _bookingRecordsManager.Confirm(bookingResponse, booking);
            
            
            Task NotifyBookingFinalization(AccommodationBookingInfo bookingInfo) => _bookingMailingService
                .NotifyBookingFinalized(bookingInfo);


            async Task<Result<AccommodationBookingInfo>> GenerateInvoice(AccommodationBookingInfo bookingInfo)
            {
                var (_, isFailure, error) = await _documentsService.GenerateInvoice(bookingInfo.BookingDetails.ReferenceCode);
                if (isFailure)
                    return Result.Failure<AccommodationBookingInfo>(error);

                return bookingInfo;
            }


            Task<Result> SendInvoice(AccommodationBookingInfo bookingInfo) 
                => _bookingMailingService.SendInvoice(bookingInfo.BookingId, bookingInfo.AgentInformation.AgentEmail, booking.AgentId);


            void WriteFailureLog(string error) => _logger
                .LogBookingConfirmationFailure($"Booking '{booking.ReferenceCode} confirmation failed: '{error}");
        }
        
        
        public async Task ProcessBookingNotFound(Booking booking, EdoContracts.Accommodations.Booking bookingResponse)
        {
            if (_dateTimeProvider.UtcNow() < booking.Created + BookingCheckTimeout)
            {
                _logger.LogBookingResponseProcessSuccess(
                    $"The booking response with the reference code '{bookingResponse.ReferenceCode}' has not been processed due to '{BookingStatusCodes.NotFound}' status.");
            }
            else
            {
                await _bookingRecordsManager.SetStatus(booking, BookingStatuses.ManualCorrectionNeeded);
                _logger.LogBookingResponseProcessSuccess(
                    $"The booking response with the reference code '{bookingResponse.ReferenceCode}' set as needed manual processing.");
            }
        }
        
        private static readonly TimeSpan BookingCheckTimeout = TimeSpan.FromMinutes(30);
        
        private readonly ISupplierOrderService _supplierOrderService;
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly IBookingMailingService _bookingMailingService;
        private readonly ILogger<BookingChangesProcessor> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingDocumentsService _documentsService;
        private readonly IBookingCreditCardPaymentService _creditCardPaymentService;
        private readonly IBookingAccountPaymentService _accountPaymentService;
        private readonly EdoContext _context;
    }
}