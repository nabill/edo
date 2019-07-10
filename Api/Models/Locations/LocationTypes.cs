using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Models.Locations
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LocationTypes
    {
        Unknown,
        Destination,
        Hotel,
        Landmark,
        Location
    }
}
