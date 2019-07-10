using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Locations.Google
{
    public readonly struct Viewport
    {
        [JsonConstructor]
        public Viewport(GeoPoint northEast, GeoPoint southWest)
        {
            NorthEast = northEast;
            SouthWest = southWest;
        }


        [JsonProperty("northeast")]
        public GeoPoint NorthEast { get; }
        [JsonProperty("southwest")]
        public GeoPoint SouthWest { get; }
    }
}