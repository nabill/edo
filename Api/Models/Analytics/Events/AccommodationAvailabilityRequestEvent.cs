using System;

namespace HappyTravel.Edo.Api.Models.Analytics.Events
{
    public readonly struct AccommodationAvailabilityRequestEvent
    {
        public AccommodationAvailabilityRequestEvent(string id, string name, string counterpartyName, string rating,
            string country, string locality, float[] location)
        {
            Id = id;
            Name = name;
            CounterpartyName = counterpartyName;
            Rating = rating;
            Country = country;
            Locality = locality;
            Location = location;
        }

        public string Id { get; }
        public string CounterpartyName { get; }
        public string Rating { get; }
        public string Country { get; }
        public string Locality { get; }
        public float[] Location { get; }
        public string Name { get; }
    }
}
