using System;

namespace HappyTravel.Edo.Api.Models.Availabilities.Events
{
    public readonly struct AccommodationAvailabilityRequestEvent
    {
        public AccommodationAvailabilityRequestEvent(string id, string name, string rating,
            string country, string locality, Guid searchId, Guid resultId)
        {
            Id = id;
            Name = name;
            Rating = rating;
            Country = country;
            Locality = locality;
            SearchId = searchId;
            ResultId = resultId;
        }

        public string Id { get; }
        public string Rating { get; }
        public string Country { get; }
        public string Locality { get; }
        public Guid SearchId { get; }
        public Guid ResultId { get; }
        public string Name { get; }
    }
}
