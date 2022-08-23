using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups.AuditEvents
{
    public readonly struct AgentMarkupPolicyData
    {
        [JsonConstructor]
        public AgentMarkupPolicyData(int markupId, int agentId, int agencyId, decimal toValue)
        {
            MarkupId = markupId;
            AgentId = agentId;
            AgencyId = agencyId;
            ToValue = toValue;
        }


        public int MarkupId { get; }
        public int AgentId { get; }
        public int AgencyId { get; }
        public decimal ToValue { get; }
    }
}
