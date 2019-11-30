using System.Collections.Generic;
using HappyTravel.Geography;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct RichLocationInfo
    {
        [JsonConstructor]
        public RichLocationInfo(in LocationInfo location, string country, string city, string cityZone)
        {
            Address = location.Address;
            Country = country;
            CountryCode = location.CountryCode;
            City = city;
            CityCode = location.CityCode;
            CityZone = cityZone;
            CityZoneCode = location.CityZoneCode;
            Coordinates = location.Coordinates;
            IsHistoricalBuilding = location.IsHistoricalBuilding;
            LocationDescriptionCode = location.LocationDescriptionCode;
            Directions = location.Directions;
        }


        public string Address { get; }
        public string Country { get; }
        public string CountryCode { get; }
        public string City { get; }
        public string CityCode { get; }
        public string CityZone { get; }
        public string CityZoneCode { get; }
        public GeoPoint Coordinates { get; }
        public bool IsHistoricalBuilding { get; }
        public AccommodationLocationDescriptionCodes LocationDescriptionCode { get; }

        /// <summary>
        /// List of transportation facility or POI
        /// </summary>
        public List<DirectionInfo> Directions { get; }
    }
}
