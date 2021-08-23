namespace HappyTravel.Edo.Api.Models.Analytics
{
    public readonly struct BookingConfirmationEvent
    {
        public BookingConfirmationEvent(string accommodationId, string accommodationName, string country, string locality, int adultCount,
            int childrenCount, int numberOfNights, int roomCount, string htId, decimal priceInUsd, string supplier)
        {
            AccommodationId = accommodationId;
            AccommodationName = accommodationName;
            Country = country;
            Locality = locality;
            AdultCount = adultCount;
            ChildrenCount = childrenCount;
            NumberOfNights = numberOfNights;
            RoomCount = roomCount;
            HtId = htId;
            PriceInUsd = priceInUsd;
            Supplier = supplier;
        }
        
        
        public string AccommodationId { get; }
        public string AccommodationName { get; }
        public string Country { get; }
        public string Locality { get; }
        public int AdultCount { get; }
        public int ChildrenCount { get; }
        public int NumberOfNights { get; }
        public int RoomCount { get; }
        public string HtId { get; }
        public decimal PriceInUsd { get; }
        public string Supplier { get; }
    }
}