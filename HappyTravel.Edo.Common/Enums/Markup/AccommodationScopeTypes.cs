using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums.Markup
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AccommodationScopeType
    {
        NotSpecified = 0,
        Global = 1,
        Country = 2,
        City = 3,
        Accommodation = 4
    }
}