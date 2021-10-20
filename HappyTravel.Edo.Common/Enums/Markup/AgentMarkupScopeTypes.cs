using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums.Markup
{
    [JsonConverter((typeof(StringEnumConverter)))]
    public enum AgentMarkupScopeTypes
    {
        NotSpecified = 0,
        Global = 1,
        Country = 2,
        City = 3,
        Counterparty = 4, // 2
        Agency = 5, // 3
        Agent = 6 // 4
    }
}