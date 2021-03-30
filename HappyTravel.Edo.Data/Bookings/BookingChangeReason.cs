using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Bookings
{
    public class BookingChangeReason
    {
        public BookingChangeInitiators Initiator { get; set; }
        public BookingChangeSources Source { get; set; }
        public BookingChangeEvents Event { get; set; }
        public string Reason { get; set; }
    }
}
