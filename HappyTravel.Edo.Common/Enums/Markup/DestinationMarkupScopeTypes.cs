using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums.Markup
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DestinationMarkupScopeTypes
    {
        NotSpecified = 0,
        Global = 1,
        Country = 2,
        Locality = 3,
        Accommodation = 4,
        Market = 5
    }
}