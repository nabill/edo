using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Booking
{
    public class AccomodationBookingRoomDetails
    {
        public int Id { get; set; }
        
        public int AccomodationBookingId { get; set; }
        public RoomTypes Type { get; set; }
        public bool IsExtraBedNeeded { get; set; }
        public bool IsCotNeededNeeded { get; set; }
        public decimal Price { get; set; }
        public decimal ExtraBedPrice { get; set; }
        public decimal CotPrice { get; set; }
    }
}