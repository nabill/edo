using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Currencies
    {
        NotSpecified = 0,
        USD = 1,
        EUR = 2
    }
}