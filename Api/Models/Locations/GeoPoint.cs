using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Locations
{
    [JsonConverter(typeof(GeoPointJsonConverter))]
    public readonly struct GeoPoint
    {
        [JsonConstructor]
        public GeoPoint([Range(-180, 180)] double longitude, [Range(-90, 90)] double latitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        [JsonProperty("lat")]
        public double Latitude { get; }
        
        [JsonProperty("lng")]
        public double Longitude { get; }
    }
}
