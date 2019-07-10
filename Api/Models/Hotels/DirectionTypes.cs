using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Api.Models.Hotels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DirectionTypes
    {
        Airport = 1,
        Bus = 2,
        Center = 3,
        Fair = 4,
        Metro = 5,
        Station = 6
    }
}
