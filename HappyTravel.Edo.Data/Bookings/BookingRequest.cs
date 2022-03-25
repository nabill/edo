namespace HappyTravel.Edo.Data.Bookings
{
    public class BookingRequest
    {
        public string ReferenceCode { get; set; } = string.Empty;
        public string? RequestData { get; set; }
        public string? AvailabilityData { get; set; }
    }
}