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


        public Location(GeoPoint coordinates, int distance, LocationTypes type = LocationTypes.Unknown) 
            : this(string.Empty, string.Empty, string.Empty, coordinates, distance, PredictionSources.NotSpecified, type)
        { }


        public GeoPoint Coordinates { get; }
        public string Country { get; }
        public int Distance { get; }
        public string Locality { get; }
        public string Name { get; }
        public PredictionSources Source { get; }
        public LocationTypes Type { get; }


        public override bool Equals(object obj) => obj is GeoPoint point && Equals(point);


        public bool Equals(Location other)
            => (Coordinates, Country, Distance, Locality, Name, Source, Type) == (other.Coordinates, other.Country, other.Distance, other.Locality, other.Name,
                other.Source, other.Type);


        public override int GetHashCode() => (Coordinates, Country, Distance, Locality, Name, Source, Type).GetHashCode();
    }
}