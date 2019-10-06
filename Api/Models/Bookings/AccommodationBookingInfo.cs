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

        public int BookingId { get; }
        public string BookingDetails { get; }
        public string ServiceDetails { get; }
        public int CompanyId { get; }
    }
}