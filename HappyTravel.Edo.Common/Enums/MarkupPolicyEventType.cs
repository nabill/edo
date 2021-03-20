using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HappyTravel.Edo.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MarkupPolicyEventType
    {
        None = 0,
        AgentMarkupCreated = 1,
        AgentMarkupUpdated = 2,
        AgentMarkupDeleted = 3,
        AgencyMarkupCreated = 4,
        AgencyMarkupUpdated = 5,
        AgencyMarkupDeleted = 6,
        CounterpartyMarkupCreated = 7,
        CounterpartyMarkupUpdated = 8,
        CounterpartyMarkupDeleted = 9,
        GlobalMarkupCreated = 10,
        GlobalMarkupUpdated = 11,
        GlobalMarkupDeleted = 12,
    }
}
