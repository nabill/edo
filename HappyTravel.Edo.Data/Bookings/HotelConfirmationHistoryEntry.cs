using HappyTravel.Edo.Common.Enums;
using System;

namespace HappyTravel.Edo.Data.Bookings
{
    public class HotelConfirmationHistoryEntry
    {
        public int Id { get; set; }
        public string ReferenceCode { get; set; }
        public HotelConfirmationStatuses Status { get; set; }
        public string Initiator { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
