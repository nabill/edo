using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Data.Booking
{
    public class AccomodationBookingPassenger
    {
        public PassengerTitle Title { get; set; }
        public string LastName { get; set; }
        public bool IsLeader { get; set; }
        public string FirstName { get; set; }
        public string Initials { get; set; }
        public int? Age { get; set; }
    }
}