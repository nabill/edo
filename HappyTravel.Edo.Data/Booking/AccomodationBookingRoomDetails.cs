using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Booking
{
    public class AccomodationBookingRoomDetails
    {
        public RoomTypes Type { get; set; }
        public bool IsExtraBedNeeded { get; set; }
        public bool IsCotNeededNeeded { get; set; }
        public decimal Price { get; set; }
        public decimal ExtraBedPrice { get; set; }
        public decimal CotPrice { get; set; }
        public List<AccomodationBookingPassenger> Passengers { get; set; }
    }
}