namespace HappyTravel.Edo.Api.Models.Bookings
{
    public struct AccommodationBookingInfo
    {
        public AccommodationBookingInfo(string bookingDetails, string serviceDetails, int companyId)
        {
            BookingDetails = bookingDetails;
            ServiceDetails = serviceDetails;
            CompanyId = companyId;
        }
        
        public string BookingDetails { get; }
        public string ServiceDetails { get; }
        public int CompanyId { get; }
    }
}