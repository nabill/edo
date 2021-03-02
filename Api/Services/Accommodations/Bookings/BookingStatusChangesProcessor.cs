using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingStatusChangesProcessor : IBookingStatusChangesProcessor
    {
        public BookingStatusChangesProcessor(IBookingInfoService bookingInfoService,
            IBookingRecordManager recordManager,
            IBookingNotificationService notificationService,
            ISupplierOrderService supplierOrderService,
            IBookingDocumentsMailingService documentsMailingService,
            IBookingMoneyReturnService moneyReturnService,
            EdoContext context,
            ILogger<BookingStatusChangesProcessor> logger)
        {
            _bookingInfoService = bookingInfoService;
            _recordManager = recordManager;
            _notificationService = notificationService;
            _supplierOrderService = supplierOrderService;
            _documentsMailingService = documentsMailingService;
            _moneyReturnService = moneyReturnService;
            _context = context;
            _logger = logger;
        }
        
        
        public async Task ProcessConfirmation(Edo.Data.Bookings.Booking booking)
        {
            await GetBookingInfo(booking.ReferenceCode, booking.LanguageCode)
                .Tap(SetConfirmationDate)
                .Tap(NotifyBookingFinalization)
                .Bind(SendInvoice)
                .OnFailure(WriteFailureLog);
            
            
            Task<Result<AccommodationBookingInfo>> GetBookingInfo(string referenceCode, string languageCode) 
                => _bookingInfoService.GetAccommodationBookingInfo(referenceCode, languageCode);


            Task SetConfirmationDate(AccommodationBookingInfo _) 
                => _recordManager.SetConfirmationDate(booking);


            Task NotifyBookingFinalization(AccommodationBookingInfo bookingInfo) 
                => _notificationService.NotifyBookingFinalized(bookingInfo);


            async Task<Result> SendInvoice(AccommodationBookingInfo bookingInfo)
            {
                // Booking was updated so we need to get it again
                var (_, _, updatedBooking, _) = await _recordManager.Get(bookingInfo.BookingId);
                return await _documentsMailingService.SendInvoice(updatedBooking, bookingInfo.AgentInformation.AgentEmail, true);
            }


            void WriteFailureLog(string error) 
                => _logger.LogBookingConfirmationFailure($"Booking '{booking.ReferenceCode} confirmation failed: '{error}");
        }
        
        
        public Task<Result> ProcessCancellation(Edo.Data.Bookings.Booking booking, UserInfo user)
        {
            return SendNotifications()
                .Tap(CancelSupplierOrder)
                .Bind(() => ReturnMoney(booking, user));

            
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
                await _notificationService.NotifyBookingCancelled(bookingInfo);
                
                return Result.Success();
            }
        }


        public Task ProcessRejection(Edo.Data.Bookings.Booking booking, UserInfo user)
        {
            return CancelSupplierOrder()
                .Bind(() => ReturnMoney(booking, user));

            
            async Task<Result> CancelSupplierOrder()
            {
                await _supplierOrderService.Cancel(booking.ReferenceCode);
                return Result.Success();
            }
        }


        public Task<Result> ProcessDiscarding(Booking booking, UserInfo user)
        {
            return CancelSupplierOrder()
                .Bind(() => ReturnMoney(booking, user));
            
            
            async Task<Result> CancelSupplierOrder()
            {
                await _supplierOrderService.Cancel(booking.ReferenceCode);
                return Result.Success();
            }
        }

        
        private Task<Result> ReturnMoney(Booking booking, UserInfo user) 
            => _moneyReturnService.ReturnMoney(booking, user);
        
        
        private readonly IBookingInfoService _bookingInfoService;
        private readonly IBookingRecordManager _recordManager;
        private readonly IBookingNotificationService _notificationService;
        private readonly ISupplierOrderService _supplierOrderService;
        private readonly IBookingDocumentsMailingService _documentsMailingService;
        private readonly IBookingMoneyReturnService _moneyReturnService;
        private readonly EdoContext _context;
        private readonly ILogger<BookingStatusChangesProcessor> _logger;
    }
}