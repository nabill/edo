using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Mailing;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.EdoContracts.Accommodations.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingChangesProcessor
    {
        public BookingChangesProcessor(ISupplierOrderService supplierOrderService,
            IBookingRecordsManager bookingRecordsManager,
            IBookingPaymentService paymentService,
            IBookingMailingService bookingMailingService,
            ILogger<BookingChangesProcessor> logger,
            IDateTimeProvider dateTimeProvider,
            EdoContext context)
        {
            _supplierOrderService = supplierOrderService;
            _bookingRecordsManager = bookingRecordsManager;
            _paymentService = paymentService;
            _bookingMailingService = bookingMailingService;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _context = context;
        }
        
        public Task<Result> ProcessCancellation(Data.Booking.Booking booking, UserInfo user)
        {
            return SendNotifications()
                .Tap(CancelSupplierOrder)
                .Bind(VoidMoney)
                .Tap(SetBookingCancelled);

            
            async Task CancelSupplierOrder()
            {
                var referenceCode = booking.ReferenceCode;
                await _supplierOrderService.Cancel(referenceCode);
            }


            async Task<Result> SendNotifications()
            {
                var agent = await _context.Agents.SingleOrDefaultAsync(a => a.Id == booking.AgentId);
                if (agent == default)
                {
                    _logger.LogWarning("Booking cancellation notification: could not find agent with id '{0}' for the booking '{1}'",
                        booking.AgentId, booking.ReferenceCode);
                    return Result.Success();
                }

                await _bookingMailingService.NotifyBookingCancelled(booking.ReferenceCode, agent.Email, $"{agent.LastName} {agent.FirstName}");
                return Result.Success();
            }

            async Task<Result> VoidMoney()
            {
                if (booking.PaymentStatus == BookingPaymentStatuses.Authorized || booking.PaymentStatus == BookingPaymentStatuses.Captured)
                    return await _paymentService.VoidOrRefund(booking, user);

                return Result.Success();
            }


            Task SetBookingCancelled() => _bookingRecordsManager.ConfirmBookingCancellation(booking);
        }
        
        
        public async Task ProcessConfirmation(Data.Booking.Booking booking, EdoContracts.Accommodations.Booking bookingResponse)
        {
            await _bookingRecordsManager.Confirm(bookingResponse, booking);
            await SaveSupplierOrder();
            
            async Task SaveSupplierOrder()
            {
                var supplierPrice = bookingResponse.RoomContractSet.Price.NetTotal;
                await _supplierOrderService.Add(bookingResponse.ReferenceCode, ServiceTypes.HTL, supplierPrice);
            }
        }
        
        
        public async Task ProcessBookingNotFound(Data.Booking.Booking booking, EdoContracts.Accommodations.Booking bookingResponse)
        {
            if (_dateTimeProvider.UtcNow() < booking.Created + BookingCheckTimeout)
            {
                _logger.LogBookingResponseProcessSuccess(
                    $"The booking response with the reference code '{bookingResponse.ReferenceCode}' has not been processed due to '{BookingStatusCodes.NotFound}' status.");
            }
            else
            {
                await _bookingRecordsManager.SetNeedsManualCorrectionStatus(booking);
                _logger.LogBookingResponseProcessSuccess(
                    $"The booking response with the reference code '{bookingResponse.ReferenceCode}' set as needed manual processing.");
            }
        }
        
        private static readonly TimeSpan BookingCheckTimeout = TimeSpan.FromMinutes(30);
        
        private readonly ISupplierOrderService _supplierOrderService;
        private readonly IBookingRecordsManager _bookingRecordsManager;
        private readonly IBookingPaymentService _paymentService;
        private readonly IBookingMailingService _bookingMailingService;
        private readonly ILogger<BookingChangesProcessor> _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly EdoContext _context;
    }
}