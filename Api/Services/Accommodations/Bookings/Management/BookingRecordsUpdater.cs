using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public class BookingRecordsUpdater : IBookingRecordsUpdater
    {
        public BookingRecordsUpdater(IDateTimeProvider dateTimeProvider,
            IBookingInfoService infoService,
            IBookingNotificationService notificationService,
            IBookingMoneyReturnService moneyReturnService,
            IBookingDocumentsMailingService documentsMailingService,
            ISupplierOrderService supplierOrderService,
            EdoContext context,
            ILogger<BookingRecordsUpdater> logger,
            IBookingNotificationService bookingNotificationService)
        {
            _dateTimeProvider = dateTimeProvider;
            _infoService = infoService;
            _notificationService = notificationService;
            _moneyReturnService = moneyReturnService;
            _documentsMailingService = documentsMailingService;
            _supplierOrderService = supplierOrderService;
            _context = context;
            _logger = logger;
            _bookingNotificationService = bookingNotificationService;
        }
        

        public async Task<Result> ChangeStatus(Booking booking, BookingStatuses status, DateTime date, UserInfo user, BookingChangeReason reason) 
        {
            if (booking.Status == status)
                return Result.Success();

            await SetStatus(booking, status);

            await AddEntryToStatusHistory(booking, status, date, user, reason);
            
            return status switch
            {
                BookingStatuses.Confirmed => await ProcessConfirmation(booking, date),
                BookingStatuses.Cancelled => await ProcessCancellation(booking, date, user),
                BookingStatuses.Rejected => await ProcessDiscarding(booking, user),
                BookingStatuses.Invalid => await ProcessDiscarding(booking, user),
                BookingStatuses.Discarded => await ProcessDiscarding(booking, user),
                BookingStatuses.ManualCorrectionNeeded => await ProcessManualCorrectionNeeding(booking, user),
                BookingStatuses.PendingCancellation => Result.Success(),
                BookingStatuses.WaitingForResponse => Result.Success(),
                BookingStatuses.Pending => Result.Success(),
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Invalid status value")
            };
        }


        public async Task UpdateWithSupplierData(Booking booking, string supplierReferenceCode, BookingUpdateModes updateModes,
            List<SlimRoomOccupation> updatedRooms)
        {
            booking.SupplierReferenceCode = supplierReferenceCode;
            booking.UpdateMode = updateModes;
            booking.Rooms = UpdateSupplierReferenceCodes(booking.Rooms, updatedRooms);
            
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            _context.Entry(booking).State = EntityState.Detached;


            static List<BookedRoom> UpdateSupplierReferenceCodes(List<BookedRoom> existingRooms, List<SlimRoomOccupation> updatedRooms)
            {
                // TODO: NIJO-928 Find corresponding room in more solid way
                // We cannot find corresponding room if room count differs
                if (updatedRooms == null || existingRooms.Count != updatedRooms.Count)
                    return existingRooms;
                
                var changedBookedRooms = new List<BookedRoom>(existingRooms.Count);
                for (var i = 0; i < updatedRooms.Count; i++)
                {
                    var changedBookedRoom = new BookedRoom(existingRooms[i], updatedRooms[i].SupplierRoomReferenceCode);
                    changedBookedRooms.Add(changedBookedRoom);
                }

                return changedBookedRooms;
            }
        }
        

        private async Task<Result> ProcessConfirmation(Edo.Data.Bookings.Booking booking, DateTime confirmationDate)
        {
            return await GetBookingInfo(booking.ReferenceCode, booking.LanguageCode)
                .Tap(SetConfirmationDate)
                .Tap(NotifyBookingFinalization)
                .Bind(SendInvoice)
                .OnFailure(WriteFailureLog);
            
            
            Task<Result<AccommodationBookingInfo>> GetBookingInfo(string referenceCode, string languageCode) 
                => _infoService.GetAccommodationBookingInfo(referenceCode, languageCode);


            Task SetConfirmationDate(AccommodationBookingInfo _) 
                => this.SetConfirmationDate(booking, confirmationDate);


            Task NotifyBookingFinalization(AccommodationBookingInfo bookingInfo) 
                => _notificationService.NotifyBookingFinalized(bookingInfo);


            async Task<Result> SendInvoice(AccommodationBookingInfo bookingInfo)
            {
                // Booking was updated so we need to get it again
                var updatedBooking = await _context.Bookings.FindAsync(bookingInfo.BookingId);
                return await _documentsMailingService.SendInvoice(updatedBooking, bookingInfo.AgentInformation.AgentEmail, true);
            }


            void WriteFailureLog(string error) 
                => _logger.LogBookingConfirmationFailure($"Booking '{booking.ReferenceCode} confirmation failed: '{error}");
        }
        
        
        private Task<Result> ProcessCancellation(Booking booking, DateTime cancellationDate, UserInfo user)
        {
            return SendNotifications()
                .Tap(CancelSupplierOrder)
                .Bind(() => ReturnMoney(booking, cancellationDate, user));

            
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

                var (_, _, bookingInfo, _) = await _infoService.GetAccommodationBookingInfo(booking.ReferenceCode, booking.LanguageCode);
                await _notificationService.NotifyBookingCancelled(bookingInfo);
                
                return Result.Success();
            }
        }


        private Task<Result> ProcessDiscarding(Booking booking, UserInfo user)
        {
            return CancelSupplierOrder()
                .Bind(() => ReturnMoney(booking, _dateTimeProvider.UtcNow(), user));
            
            
            async Task<Result> CancelSupplierOrder()
            {
                await _supplierOrderService.Cancel(booking.ReferenceCode);
                return Result.Success();
            }
        }


        private async Task<Result> ProcessManualCorrectionNeeding(Booking booking, UserInfo user)
        {
            var additionalInfo = await 
                (from bookings in _context.Bookings
                    join agencies in _context.Agencies on bookings.AgencyId equals agencies.Id
                    join agents in _context.Agents on bookings.AgentId equals agents.Id
                    where bookings.Id == booking.Id
                    select new {AgentName = $"{agents.FirstName} {agents.LastName}", AgencyName = agencies.Name})
                .SingleOrDefaultAsync();
            
            if (additionalInfo is null)
                return Result.Failure($"Cannot get additional info for booking id '{booking.Id}'");
            
            await _bookingNotificationService.NotifyBookingManualCorrectionNeeded(
                booking.ReferenceCode,
                additionalInfo.AgentName,
                additionalInfo.AgencyName,
                DateTimeFormatters.ToDateString(booking.DeadlineDate ?? booking.CheckOutDate));
            return Result.Success();
        }

        
        private Task<Result> ReturnMoney(Booking booking, DateTime operationDate, UserInfo user) 
            => _moneyReturnService.ReturnMoney(booking, operationDate, user);
        
        
        private async Task SetConfirmationDate(Booking booking, DateTime date)
        {
            booking.ConfirmationDate = date;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            _context.Detach(booking);;
        }


        private async Task SetStatus(Booking booking, BookingStatuses status)
        {
            booking.Status = status;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            _context.Detach(booking);
        }


        private async Task AddEntryToStatusHistory(Booking booking, BookingStatuses status, DateTime date, UserInfo user, BookingChangeReason reason)
        {
            var bookingStatusHistoryEntry = new BookingStatusHistoryEntry
            {
                BookingId = booking.Id,
                UserId = user.Id,
                UserType = user.Type,
                CreatedAt = date,
                Status = status,
                ChangeSource = reason.ChangeSource,
                ChangeEvent = reason.ChangeEvent,
                ChangeReason = reason.ChangeReason
            };
            if (user.Type == UserTypes.Agent)
            {
                bookingStatusHistoryEntry.AgencyId = booking.AgencyId;
            }

            var entry = _context.BookingStatusHistory.Add(bookingStatusHistoryEntry);
            await _context.SaveChangesAsync();
            _context.Detach(entry.Entity);
        }


        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingInfoService _infoService;
        private readonly IBookingNotificationService _notificationService;
        private readonly IBookingMoneyReturnService _moneyReturnService;
        private readonly IBookingDocumentsMailingService _documentsMailingService;
        private readonly ISupplierOrderService _supplierOrderService;
        private readonly EdoContext _context;
        private readonly ILogger<BookingRecordsUpdater> _logger;
        private readonly IBookingNotificationService _bookingNotificationService;
    }
}