using HappyTravel.Geography;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct SlimLocationInfo
    {
        [JsonConstructor]
        public SlimLocationInfo(string address, string country, string countryCode, string locality, string localityZone, in GeoPoint coordinates)
        {
            Address = address;
            Coordinates = coordinates;
            Country = country;
            CountryCode = countryCode;
            Locality = locality;
            LocalityZone = localityZone;
        }


        /// <summary>
        ///     The location address.
        /// </summary>
        public string Address { get; }

        /// <summary>
        ///     The location country name.
        /// </summary>
        public string Country { get; }

        /// <summary>
        ///     The country code in the ISO 3166-1 Alpha-2 format.
        /// </summary>
        public string CountryCode { get; }

        /// <summary>
        ///     Location coordinates.
        /// </summary>
        public GeoPoint Coordinates { get; }

        /// <summary>
        ///     The locality name.
        /// </summary>
        public string Locality { get; }

        /// <summary>
        ///     The locality zone name.
        /// </summary>
        public string LocalityZone { get; }


        public override bool Equals(object obj) => obj is SlimLocationInfo other && Equals(other);


        public bool Equals(in SlimLocationInfo other)
            => (Address, Country, CountryCode, Coordinates, Locality, LocalityZone)
                .Equals((other.Address, other.Country, other.CountryCode, other.Coordinates, other.Locality, other.LocalityZone));


        public override int GetHashCode() => (Address, Country, CountryCode, Coordinates, Locality, LocalityZone).GetHashCode();
    }
}