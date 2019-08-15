using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TextualDescriptionTypes
    {
        Exterior = 1,
        General = 2,
        Lobby = 3,
        Position = 4,
        Restaurant = 5,
        Room = 6
    }
}
