using System;
using HappyTravel.EdoContracts.Accommodations;

namespace HappyTravel.Edo.Data.Booking
{
    public class BookingRequestDataEntry
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public BookingRequest BookingRequest { get; set; }
        public string LanguageCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}