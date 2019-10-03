namespace HappyTravel.Edo.Api.Models.Bookings
{
    public struct AccommodationBookingInfo
    {
        public AccommodationBookingInfo(int bookingId, string bookingDetails, string serviceDetails, int companyId, string deadlineDetails)
        {
            BookingId = bookingId;
            BookingDetails = bookingDetails;
            ServiceDetails = serviceDetails;
            CompanyId = companyId;
            DeadlineDetails = deadlineDetails;
        }

        public int BookingId { get; }
        public string BookingDetails { get; }
        public string ServiceDetails { get; }
        public int CompanyId { get; }
        public string DeadlineDetails { get; }

    }
}