using HappyTravel.Edo.Api.Models.Locations;

namespace HappyTravel.Edo.Api.Models.Analytics.Events
{
    public readonly struct WideAvailabilityRequestEvent
    {
        public WideAvailabilityRequestEvent(string counterpartyName, Location location, int adultCount, int childrenCount,
            int numberOfNights, int roomCount)
        {
            CounterpartyName = counterpartyName;
            Location = location;
            AdultCount = adultCount;
            ChildrenCount = childrenCount;
            NumberOfNights = numberOfNights;
            RoomCount = roomCount;
        }
        
        public string CounterpartyName { get; }
        public Location Location { get; }
        public int AdultCount { get; }
        public int ChildrenCount { get; }
        public int NumberOfNights { get; }
        public int RoomCount { get; }
    }
}