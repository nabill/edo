using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.NotificationCenter.Models;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Services.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public class BookingRecordsUpdater : IBookingRecordsUpdater
    {
        public BookingRecordsUpdater(IDateTimeProvider dateTimeProvider, IBookingInfoService infoService,
            IBookingNotificationService bookingNotificationService, IBookingMoneyReturnService moneyReturnService,
            IBookingDocumentsMailingService documentsMailingService, ISupplierOrderService supplierOrderService,
            INotificationService notificationService, IBookingChangeLogService bookingChangeLogService,
            IBookingAnalyticsService bookingAnalyticsService, EdoContext context, ILogger<BookingRecordsUpdater> logger)
        {
            _dateTimeProvider = dateTimeProvider;
            _infoService = infoService;
            _bookingNotificationService = bookingNotificationService;
            _moneyReturnService = moneyReturnService;
            _documentsMailingService = documentsMailingService;
            _supplierOrderService = supplierOrderService;
            _notificationsService = notificationService;
            _context = context;
            _logger = logger;
            _bookingChangeLogService = bookingChangeLogService;
            _bookingAnalyticsService = bookingAnalyticsService;
        }


        public async Task<Result> ChangeStatus(Booking booking, BookingStatuses status, DateTimeOffset date, ApiCaller apiCaller, BookingChangeReason reason)
        {
            if (booking.Status == status)
                return Result.Success();

            booking.Status = status;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            _context.Detach(booking);

            var message = new BookingStatusChangeInfo
            {
                BookingId = booking.Id,
                ReferenceCode = booking.ReferenceCode,
                Status = EnumFormatters.FromDescription(status),
                ChangeTime = _dateTimeProvider.UtcNow(),
                AccommodationName = booking.AccommodationName,
                AccommodationPhoto = booking.AccommodationInfo?.Photo,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate
            };
            await _notificationsService.Send(apiCaller,
                JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(message, new JsonSerializerOptions(JsonSerializerDefaults.Web))),
                Notifications.Enums.NotificationTypes.BookingStatusChanged);

            // Temporary hot-fix for notifying admins about bookings statuses changed to "Pending" or "Waiting for response"
            // TODO: remove when we have appropriate admin panel booking monitoring
            if (status == BookingStatuses.Pending || status == BookingStatuses.WaitingForResponse)
            {
                var (isSuccess, _, bookingInfo, _) =
                    await _infoService.GetAccommodationBookingInfo(booking.ReferenceCode, LocalizationHelper.DefaultLanguageCode);
                if (isSuccess)
                    await _bookingNotificationService.NotifyAdminsStatusChanged(bookingInfo);
            }

            await _bookingChangeLogService.Write(booking, status, date, apiCaller, reason);

            return status switch
            {
                BookingStatuses.Confirmed => await ProcessConfirmation(booking, date),
                BookingStatuses.Cancelled => await ProcessCancellation(booking, date, apiCaller),
                BookingStatuses.Discarded => await ProcessDiscarding(booking, apiCaller),
                BookingStatuses.Rejected => await ProcessDiscarding(booking, apiCaller),
                BookingStatuses.Invalid => await ProcessDiscarding(booking, apiCaller),
                BookingStatuses.ManualCorrectionNeeded => await ProcessManualCorrectionNeeding(booking, apiCaller),
                BookingStatuses.PendingCancellation => Result.Success(),
                BookingStatuses.WaitingForResponse => Result.Success(),
                BookingStatuses.Pending => Result.Success(),
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Invalid status value")
            };
        }


        public async Task ChangePropertyOwnerConfirmationCode(Booking booking, string confirmationCode)
        {
            if (booking.PropertyOwnerConfirmationCode == confirmationCode)
                return;

            booking.PropertyOwnerConfirmationCode = confirmationCode;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            _context.Detach(booking);
        }


        public async Task UpdateWithSupplierData(Booking booking, string supplierReferenceCode, BookingUpdateModes updateModes,
            List<SlimRoomOccupation> updatedRooms, List<KeyValuePair<string, string>> specialValues)
        {
            booking.SupplierReferenceCode = supplierReferenceCode;
            booking.UpdateMode = updateModes;
            booking.Rooms = UpdateSupplierReferenceCodes(booking.Rooms, updatedRooms);
            booking.SpecialValues = specialValues;

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            _context.Detach(booking);


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


        private async Task<Result> ProcessConfirmation(Booking booking, DateTimeOffset confirmationDate)
        {
            return await _infoService.GetAccommodationBookingInfo(booking.ReferenceCode, booking.LanguageCode)
                .Tap(SetConfirmationDate)
                .Tap(NotifyBookingFinalization)
                .Tap(LogAnalyticsConfirmed)
                .Bind(SendInvoice)
                .OnFailure(WriteFailureLog);


            async Task SetConfirmationDate()
            {
                booking.ConfirmationDate = confirmationDate;
                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();
                _context.Detach(booking);
            }


            Task NotifyBookingFinalization(AccommodationBookingInfo bookingInfo)
                => _bookingNotificationService.NotifyBookingFinalized(bookingInfo, new SlimAgentContext(booking.AgentId, booking.AgencyId));


            async Task<Result> SendInvoice(AccommodationBookingInfo bookingInfo)
            {
                // Booking was updated so we need to get it again
                var updatedBooking = await _context.Bookings.FindAsync(bookingInfo.BookingId);
                return await _documentsMailingService.SendInvoice(updatedBooking!, bookingInfo.AgentInformation.AgentEmail, true,
                    new SlimAgentContext(booking.AgentId, booking.AgencyId));
            }


            async Task LogAnalyticsConfirmed()
            {
                var agency = await _context.Agencies
                    .SingleOrDefaultAsync(x => x.Id == booking.AgencyId);

                _bookingAnalyticsService.LogBookingConfirmed(booking, agency.Name);
            }


            void WriteFailureLog(string error) => _logger.LogBookingConfirmationFailure(booking.ReferenceCode, error);
        }


        private Task<Result> ProcessCancellation(Booking booking, DateTimeOffset cancellationDate, ApiCaller user)
        {
            return _infoService.GetAccommodationBookingInfo(booking.ReferenceCode, booking.LanguageCode)
                .Bind(_ => Result.Success(booking))
                .Bind(SendCancellationNotifications)
                .Tap(SetCancellationDate)
                .Tap(CancelSupplierOrder)
                .Tap(LogAnalyticsCancelled)
                .Bind(b => _moneyReturnService.ReturnMoney(b, cancellationDate, user));


            async Task SetCancellationDate()
            {
                booking.Cancelled = cancellationDate;
                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();
                _context.Detach(booking);
            }


            Task CancelSupplierOrder() => _supplierOrderService.Cancel(booking.ReferenceCode);


            async Task LogAnalyticsCancelled()
            {
                var agency = await _context.Agencies
                    .SingleOrDefaultAsync(x => x.Id == booking.AgencyId);

                _bookingAnalyticsService.LogBookingCancelled(booking, agency.Name);
            }
        }


        private Task<Result> ProcessDiscarding(Booking booking, ApiCaller user)
        {
            return DiscardSupplierOrder()
                .Bind(SendCancellationNotifications)
                .Bind(b => _moneyReturnService.ReturnMoney(b, _dateTimeProvider.UtcNow(), user));


            async Task<Result<Booking>> DiscardSupplierOrder()
            {
                await _supplierOrderService.Discard(booking.ReferenceCode);
                return Result.Success(booking);
            }
        }


        private async Task<Result<Booking>> SendCancellationNotifications(Booking booking)
        {
            var agent = await _context.Agents.SingleOrDefaultAsync(a => a.Id == booking.AgentId);
            if (agent == default)
            {
                _logger.LogWarning("Booking cancellation notification: could not find agent with id '{0}' for the booking '{1}'",
                    booking.AgentId, booking.ReferenceCode);

                return Result.Success(booking);
            }

            var (_, _, bookingInfo, _) = await _infoService.GetAccommodationBookingInfo(booking.ReferenceCode, booking.LanguageCode);
            await _bookingNotificationService.NotifyBookingCancelled(bookingInfo, new SlimAgentContext(booking.AgentId, booking.AgencyId));

            return Result.Success(booking);
        }


        private async Task<Result> ProcessManualCorrectionNeeding(Booking booking, ApiCaller user)
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


        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IBookingInfoService _infoService;
        private readonly IBookingNotificationService _bookingNotificationService;
        private readonly IBookingMoneyReturnService _moneyReturnService;
        private readonly IBookingDocumentsMailingService _documentsMailingService;
        private readonly ISupplierOrderService _supplierOrderService;
        private readonly INotificationService _notificationsService;
        private readonly IBookingAnalyticsService _bookingAnalyticsService;
        private readonly EdoContext _context;
        private readonly ILogger<BookingRecordsUpdater> _logger;
        private readonly IBookingChangeLogService _bookingChangeLogService;
    }
}