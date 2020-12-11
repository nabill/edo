using HappyTravel.Edo.Api.Models.Locations;

namespace HappyTravel.Edo.Api.Models.Analytics.Events
{
    public readonly struct WideAvailabilityRequestEvent
    {
        public WideAvailabilityRequestEvent(string counterpartyName, int adultCount, int childrenCount,
            int numberOfNights, int roomCount, string country, string locality, string locationName, string locationType,
            float[] location)
        {
            CounterpartyName = counterpartyName;
            AdultCount = adultCount;
            ChildrenCount = childrenCount;
            NumberOfNights = numberOfNights;
            RoomCount = roomCount;
            Country = country;
            Locality = locality;
            LocationName = locationName;
            LocationType = locationType;
            Location = location;
        }
        
        public string CounterpartyName { get; }
        public int AdultCount { get; }
        public int ChildrenCount { get; }
        public int NumberOfNights { get; }
        public int RoomCount { get; }
        public string Country { get; }
        public string Locality { get; }
        public string LocationName { get; }
        public string LocationType { get; }
        public float[] Location { get; }
    }
}