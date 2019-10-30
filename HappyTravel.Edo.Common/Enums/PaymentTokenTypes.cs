using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentTokenTypes
    {
        OneTime = 0,
        Stored = 1
    }
}
