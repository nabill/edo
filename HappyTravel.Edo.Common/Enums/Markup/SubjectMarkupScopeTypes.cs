using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums.Markup
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SubjectMarkupScopeTypes
    {
        NotSpecified = 0,
        Global = 1,
        Country = 2,
        Locality = 3,
        Counterparty = 4,
        Agency = 5,
        Agent = 6
    }
}