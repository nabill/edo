using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums.Markup
{
    [JsonConverter((typeof(StringEnumConverter)))]
    public enum AgentMarkupScopeTypes
    {
        NotSpecified = 0,
        Global = 1,
        Counterparty = 2,
        Agency = 3,
        Agent = 4,
        Location = 5
    }
}