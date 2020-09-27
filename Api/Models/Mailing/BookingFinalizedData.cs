using HappyTravel.Edo.Api.Models.Bookings;

namespace HappyTravel.Edo.Api.Models.Mailing
{
    public class BookingFinalizedData : DataWithCompanyInfo
    {
        public string AgentName { get; set; }
        public AccommodationBookingDetails BookingDetails { get; set; }
        public string CounterpartyName { get; set; }
        public string PaymentStatus { get; set; }
        public string Price { get; set; }
    }
}
