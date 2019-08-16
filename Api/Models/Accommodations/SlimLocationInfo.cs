using HappyTravel.Edo.Api.Models.Locations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct SlimLocationInfo
    {
        [JsonConstructor]
        public SlimLocationInfo(string address, string countryCode, string country, string cityCode, string city, string cityZoneCode, string cityZone,
            GeoPoint coordinates)
        {
            Address = address;
            Country = country;
            CountryCode = countryCode;
            City = city;
            CityCode = cityCode;
            CityZone = cityZone;
            CityZoneCode = cityZoneCode;
            Coordinates = coordinates;
        }


        public string Address { get; }
        public string Country { get; }
        public string CountryCode { get; }
        public string City { get; }
        public string CityCode { get; }
        public string CityZone { get; }
        public string CityZoneCode { get; }
        public GeoPoint Coordinates { get; }
    }
}
