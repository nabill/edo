using System;

namespace HappyTravel.Edo.Api.Models.Availabilities.Events
{
    public readonly struct AccommodationBookingEvent
    {
        public AccommodationBookingEvent(string accommodationId, string accommodationName, string country, string locality, int adultCount,
            int childrenCount, int numberOfNights, int roomCount, Guid searchId, Guid resultId, Guid roomContractSetId)
        {
            AccommodationId = accommodationId;
            AccommodationName = accommodationName;
            Country = country;
            Locality = locality;
            AdultCount = adultCount;
            ChildrenCount = childrenCount;
            NumberOfNights = numberOfNights;
            RoomCount = roomCount;
            SearchId = searchId;
            ResultId = resultId;
            RoomContractSetId = roomContractSetId;
        }
        
        
        public string AccommodationId { get; }
        public string AccommodationName { get; }
        public string Country { get; }
        public string Locality { get; }
        public int AdultCount { get; }
        public int ChildrenCount { get; }
        public int NumberOfNights { get; }
        public int RoomCount { get; }
        public Guid SearchId { get; }
        public Guid ResultId { get; }
        public Guid RoomContractSetId { get; }
    }
}