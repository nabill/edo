using System;

namespace HappyTravel.Edo.Api.Models.Analytics
{
    public readonly struct WideAvailabilityRequestEvent
    {
        public WideAvailabilityRequestEvent(int adultCount, int childrenCount,
            int numberOfNights, int roomCount, string country, string locality, string locationName,
            string locationType, Guid searchId, string language)
        {
            AdultCount = adultCount;
            ChildrenCount = childrenCount;
            NumberOfNights = numberOfNights;
            RoomCount = roomCount;
            Country = country;
            Locality = locality;
            LocationName = locationName;
            LocationType = locationType;
            SearchId = searchId;
            Language = language;
        }
        
        public int AdultCount { get; }
        public int ChildrenCount { get; }
        public int NumberOfNights { get; }
        public int RoomCount { get; }
        public string Country { get; }
        public string Locality { get; }
        public string LocationName { get; }
        public string LocationType { get; }
        public Guid SearchId { get; }
        public string Language { get; }
    }
}