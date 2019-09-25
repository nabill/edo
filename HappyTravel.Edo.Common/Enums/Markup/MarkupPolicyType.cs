using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums.Markup
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MarkupPolicyType
    {
        NotSpecified = 0,
        Multiplication = 1,
        Addition = 2
    }
}