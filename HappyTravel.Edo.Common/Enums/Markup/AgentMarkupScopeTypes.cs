using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums.Markup
{
    [JsonConverter((typeof(StringEnumConverter)))]
    // TODO reorder enum, move Location after Global https://github.com/happy-travel/agent-app-project/issues/736
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