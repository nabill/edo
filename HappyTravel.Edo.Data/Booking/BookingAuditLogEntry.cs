using System;

namespace HappyTravel.Edo.Data.Booking
{
    public class BookingAuditLogEntry
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int AgentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public EdoContracts.Accommodations.Booking  BookingDetails {get; set; }
    }
}