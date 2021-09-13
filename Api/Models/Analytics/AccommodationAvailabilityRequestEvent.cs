using System;

namespace HappyTravel.Edo.Api.Models.Analytics
{
    public readonly struct AccommodationAvailabilityRequestEvent
    {
        public AccommodationAvailabilityRequestEvent(string name, string rating,
            string country, string locality, Guid searchId, string htId)
        {
            Name = name;
            Rating = rating;
            Country = country;
            Locality = locality;
            SearchId = searchId;
            HtId = htId;
        }

        public string Rating { get; }
        public string Country { get; }
        public string Locality { get; }
        public Guid SearchId { get; }
        public string HtId { get; }
        public string Name { get; }
    }
}
