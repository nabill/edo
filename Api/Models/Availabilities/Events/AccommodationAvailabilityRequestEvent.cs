namespace HappyTravel.Edo.Api.Models.Availabilities.Events
{
    public readonly struct AccommodationAvailabilityRequestEvent
    {
        public AccommodationAvailabilityRequestEvent(string id, string name, string rating,
            string country, string locality)
        {
            Id = id;
            Name = name;
            Rating = rating;
            Country = country;
            Locality = locality;
        }

        public string Id { get; }
        public string Rating { get; }
        public string Country { get; }
        public string Locality { get; }
        public string Name { get; }
    }
}
