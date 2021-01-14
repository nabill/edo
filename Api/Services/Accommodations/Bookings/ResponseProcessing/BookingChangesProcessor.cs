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
            IBookingRecordManager bookingRecordManager,
            IBookingNotificationService bookingNotificationService,
            ILogger<BookingChangesProcessor> logger,
            IDateTimeProvider dateTimeProvider,
            IBookingCreditCardPaymentService creditCardPaymentService,
            IBookingAccountPaymentService accountPaymentService,
            IBookingInfoService bookingInfoService,
            IBookingDocumentsMailingService documentsMailingService,
            EdoContext context)
        {
            _supplierOrderService = supplierOrderService;
            _bookingRecordManager = bookingRecordManager;
            _bookingNotificationService = bookingNotificationService;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _creditCardPaymentService = creditCardPaymentService;
            _accountPaymentService = accountPaymentService;
            _bookingInfoService = bookingInfoService;
            _documentsMailingService = documentsMailingService;
            _context = context;
        }
        
        public Task<Result> ProcessCancellation(Booking booking, UserInfo user)
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
        
        
        public async Task ProcessConfirmation(Booking booking, EdoContracts.Accommodations.Booking bookingResponse)
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
                => _documentsMailingService.SendInvoice(bookingInfo.BookingId, bookingInfo.AgentInformation.AgentEmail, booking.AgentId);


            void WriteFailureLog(string error) 
                => _logger.LogBookingConfirmationFailure($"Booking '{booking.ReferenceCode} confirmation failed: '{error}");
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
                await _bookingRecordManager.SetStatus(booking.ReferenceCode, BookingStatuses.ManualCorrectionNeeded);
                _logger.LogBookingResponseProcessSuccess(
                    $"The booking response with the reference code '{bookingResponse.ReferenceCode}' set as needed manual processing.");
            }
        }


        public Task ProcessRejection(Booking booking, UserInfo user)
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


        public Task<Result> ProcessDiscarding(Booking booking, UserInfo user)
        {
            return CancelSupplierOrder()
                .Bind(() => ReturnMoney(booking, user))
                .Tap(SetBookingDiscarded);
            
            async Task<Result> CancelSupplierOrder()
            {
                await _supplierOrderService.Cancel(booking.ReferenceCode);
                return Result.Success();
            }
            
            Task SetBookingDiscarded() 
                => _bookingRecordManager.SetStatus(booking.ReferenceCode, BookingStatuses.Discarded);
        }


        private async Task<Result> ReturnMoney(Booking booking, UserInfo user)
        {
            return booking.PaymentMethod switch
            {
                PaymentMethods.BankTransfer => await ReturnBankTransfer(),
                PaymentMethods.CreditCard => await ReturnCreditCardPayment(),
                PaymentMethods.Offline => Result.Success(),
                _ => throw new ArgumentOutOfRangeException(nameof(booking.PaymentMethod), $"Invalid payment method {booking.PaymentMethod}")
            };


            async Task<Result> ReturnBankTransfer()
                => booking.PaymentStatus switch
                {
                    BookingPaymentStatuses.NotPaid => Result.Success(),
                    BookingPaymentStatuses.Refunded => Result.Success(),
                    BookingPaymentStatuses.Captured => await _accountPaymentService.Refund(booking, user),
                    _ => throw new ArgumentOutOfRangeException(nameof(booking.PaymentStatus), $"Invalid payment status {booking.PaymentStatus}")
                };


            async Task<Result> ReturnCreditCardPayment()
                => booking.PaymentStatus switch
                {
                    BookingPaymentStatuses.Refunded => Result.Success(),
                    BookingPaymentStatuses.Voided => Result.Success(),
                    BookingPaymentStatuses.Authorized => await _creditCardPaymentService.Void(booking, user),
                    BookingPaymentStatuses.Captured => await _creditCardPaymentService.Refund(booking, user),
                    _ => throw new ArgumentOutOfRangeException(nameof(booking.PaymentStatus), $"Invalid payment status {booking.PaymentStatus}")
                };
        }


        private static readonly TimeSpan BookingCheckTimeout = TimeSpan.FromMinutes(30);
        
        private readonly ISupplierOrderService _supplierOrderService;
        private readonly IBookingRecordManager _bookingRecordManager;
        private readonly IBookingNotificationService _bookingNotificationService;
        private readonly ILogger<BookingChangesProcessor> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingCreditCardPaymentService _creditCardPaymentService;
        private readonly IBookingAccountPaymentService _accountPaymentService;
        private readonly IBookingInfoService _bookingInfoService;
        private readonly IBookingDocumentsMailingService _documentsMailingService;
        private readonly EdoContext _context;
    }
}