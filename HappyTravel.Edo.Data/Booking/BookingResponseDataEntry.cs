using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Data.Booking
{
    public class BookingResponseDataEntry
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public BookingDetails BookingDetails {get; set; }
    }
}