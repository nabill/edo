using System;

namespace HappyTravel.Edo.Api.Models.Analytics
{
    public readonly struct BookingStatusChangeEvent
    {
        public BookingStatusChangeEvent(string accommodationId, string accommodationName, string country, string locality, int adultCount,
            int childrenCount, int numberOfNights, int roomCount, string htId, string status, decimal priceInUsd, string supplier)
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
            Status = status;
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
        public string Status { get; }
        public decimal PriceInUsd { get; }
        public string Supplier { get; }
    }
}