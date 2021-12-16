using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Static
{
    public readonly struct GeoPoint
    {
        [JsonConstructor]
        public GeoPoint(double longitude, double latitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        
        
        [Range(-90, 90)]
        public double Latitude { get; }
        
        [Range(-180, 180)]
        public double Longitude { get; }
    }
}