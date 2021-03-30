using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Bookings
{
    public class BookingChangeReason
    {
        public string Initiator { get; set; }
        public ChangeSources Source { get; set; }
        public BookingChangeEvents Event { get; set; }
        public string Reason { get; set; }
    }
}
