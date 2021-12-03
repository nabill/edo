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
        
        
        public double Latitude { get; }
        public double Longitude { get; }
    }
}