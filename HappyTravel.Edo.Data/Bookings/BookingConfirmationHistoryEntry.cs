using HappyTravel.Edo.Common.Enums;
using System;

namespace HappyTravel.Edo.Data.Bookings
{
    public class BookingConfirmationHistoryEntry
    {
        public int Id { get; set; }
        public string ReferenceCode { get; set; } = string.Empty;
        public string? ConfirmationCode { get; set; }
        public BookingConfirmationStatuses Status { get; set; }
        public string? Comment { get; set; }
        public string Initiator { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
