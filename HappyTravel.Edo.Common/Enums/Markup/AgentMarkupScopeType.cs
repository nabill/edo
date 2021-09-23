using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums.Markup
{
    [JsonConverter((typeof(StringEnumConverter)))]
    public enum AgentMarkupScopeType
    {
        NotSpecified = 0,
        Global = 1,
        Counterparty = 2,
        Agency = 3,
        Agent = 4,
        Country = 5,
        City = 6
    }
}