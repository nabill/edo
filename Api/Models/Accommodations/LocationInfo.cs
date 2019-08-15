using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Locations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct LocationInfo
    {
        [JsonConstructor]
        public LocationInfo(string countryCode, string cityCode, string cityZoneCode, GeoPoint coordinates, string address, bool isHistoricalBuilding,
            AccommodationLocationDescriptionCodes locationDescriptionCode, List<DirectionInfo> directions)
        {
            Address = address;
            CityCode = cityCode;
            CityZoneCode = cityZoneCode;
            Coordinates = coordinates;
            CountryCode = countryCode;
            IsHistoricalBuilding = isHistoricalBuilding;
            LocationDescriptionCode = locationDescriptionCode;
            Directions = directions ?? new List<DirectionInfo>();
        }


        public string Address { get; }
        public string CountryCode { get; }
        public string CityCode { get; }
        public string CityZoneCode { get; }
        public GeoPoint Coordinates { get; }
        public bool IsHistoricalBuilding { get; }
        public AccommodationLocationDescriptionCodes LocationDescriptionCode { get; }
        public List<DirectionInfo> Directions { get; }
    }
}
