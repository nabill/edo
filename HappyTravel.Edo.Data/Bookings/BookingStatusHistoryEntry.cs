using HappyTravel.Edo.Common.Enums;
using System;

namespace HappyTravel.Edo.Data.Bookings
{
    public class BookingStatusHistoryEntry
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public UserTypes UserType { get; set; }
        public int? AgencyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public BookingStatuses Status { get; set; }
        public ChangeInitiators Initiator { get; set; }
        public ChangeSources Source { get; set; }
        public BookingChangeEvents Event { get; set; }
        public string Reason { get; set; }
    }
}