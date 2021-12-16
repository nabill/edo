using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Static
{
    public readonly struct GeoPoint
    {
        [JsonConstructor]
        public GeoPoint([Range(-180, 180)]double longitude, [Range(-90, 90)]double latitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        
        
        public double Latitude { get; }
        public double Longitude { get; }
    }
}