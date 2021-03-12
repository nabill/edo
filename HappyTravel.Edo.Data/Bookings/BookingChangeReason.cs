using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Bookings
{
    public class BookingChangeReason
    {
        public ChangeSources ChangeSource { get; set; }
        public BookingChangeEvents ChangeEvent { get; set; }
    }
}
