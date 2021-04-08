using System;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management
{
    public class BookingChangeLogService : IBookingChangeLogService
    {
        public BookingChangeLogService(EdoContext context)
        {
            _context = context;
        }


        public async Task Write(Booking booking, BookingStatuses status, DateTime date, UserInfo user, BookingChangeReason reason)
        {
            var bookingStatusHistoryEntry = new BookingStatusHistoryEntry
            {
                BookingId = booking.Id,
                UserId = user.Id,
                UserType = user.Type,
                CreatedAt = date,
                Status = status,
                Initiator = GetInitiatorType(user),
                Source = reason.Source,
                Event = reason.Event,
                Reason = reason.Reason
            };
            
            if (user.Type == UserTypes.Agent)
                bookingStatusHistoryEntry.AgencyId = booking.AgencyId;

            var entry = _context.BookingStatusHistory.Add(bookingStatusHistoryEntry);
            await _context.SaveChangesAsync();
            _context.Detach(entry.Entity);


            static BookingChangeInitiators GetInitiatorType(UserInfo userInfo)
                => userInfo.Type switch
                {
                    UserTypes.Admin => BookingChangeInitiators.Administrator,
                    UserTypes.Agent => BookingChangeInitiators.Agent,
                    UserTypes.ServiceAccount => BookingChangeInitiators.System,
                    UserTypes.InternalServiceAccount => BookingChangeInitiators.System,
                    UserTypes.Supplier => BookingChangeInitiators.Supplier,
                    _ => throw new ArgumentOutOfRangeException()
                };
        }
        
        
        private readonly EdoContext _context;
    }
}