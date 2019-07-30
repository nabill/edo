using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Locations
{
    public readonly struct Location
    {
        public Location(string name, string locality, string country, GeoPoint coordinates, int distance, PredictionSources source, LocationTypes type)
        {
            Name = name;
            Locality = locality;
            Country = country;
            Coordinates = coordinates;
            Distance = distance;
            Source = source;
            Type = type;
        }


        public GeoPoint Coordinates { get; }
        public string Country { get; }
        public int Distance { get; }
        public string Locality { get; }
        public string Name { get; }
        public PredictionSources Source { get; }
        public LocationTypes Type { get; }
    }
}
