namespace HappyTravel.Edo.Api.Models.Bookings
{
    public struct AccommodationBookingInfo
    {
        public AccommodationBookingInfo(int bookingId, string bookingDetails, string serviceDetails, int companyId)
        {
            BookingId = bookingId;
            BookingDetails = bookingDetails;
            ServiceDetails = serviceDetails;
            CompanyId = companyId;
        }


        public override bool Equals(object obj) => obj is AccommodationBookingInfo other && Equals(other);


        public bool Equals(AccommodationBookingInfo other)
            => (BookingId, BookingDetails, ServiceDetails, CompanyId) ==
                (other.BookingId, other.BookingDetails, other.ServiceDetails, other.CompanyId);


        public override int GetHashCode() => (BookingId, BookingDetails, ServiceDetails, CompanyId).GetHashCode();


        public int BookingId { get; }
        public string BookingDetails { get; }
        public string ServiceDetails { get; }
        public int CompanyId { get; }
    }
}