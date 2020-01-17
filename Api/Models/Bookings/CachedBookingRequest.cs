namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct CachedBookingRequest
    {
        public CachedBookingRequest(AccommodationBookingRequest bookingRequest, string languageCode)
        {
            BookingRequest = bookingRequest;
            LanguageCode = languageCode;
        }
        
        public AccommodationBookingRequest BookingRequest { get; }
        public string LanguageCode { get; }
    }
}