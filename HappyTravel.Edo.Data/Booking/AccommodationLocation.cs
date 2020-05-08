using System;
using HappyTravel.Geography;

namespace HappyTravel.Edo.Data.Booking
{
    public readonly struct AccommodationLocation
    {
        public AccommodationLocation(string country, string locality, string zone, string address, GeoPoint coordinates)
        {
            Country = country;
            Locality = locality;
            Zone = zone;
            Address = address;
            Coordinates = coordinates;
        }
        
        public string Country { get; }
        public string Locality { get; }
        public string Zone { get; }
        public string Address { get; }
        public GeoPoint Coordinates { get; }


        public bool Equals(AccommodationLocation other)
            => Country == other.Country && Locality == other.Locality && Zone == other.Zone && Address == other.Address &&
                Coordinates.Equals(other.Coordinates);


        public override bool Equals(object obj) => obj is AccommodationLocation other && Equals(other);


        public override int GetHashCode() => HashCode.Combine(Country, Locality, Zone, Address, Coordinates);
    }
}