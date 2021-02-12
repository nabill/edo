using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups.AuditEvents
{
    public readonly struct AgentMarkupPolicyData
    {
        [JsonConstructor]
        public AgentMarkupPolicyData(int markupId, int agentId, int agencyId)
        {
            MarkupId = markupId;
            AgentId = agentId;
            AgencyId = agencyId;
        }


        public int MarkupId { get; }
        public int AgentId { get; }
        public int AgencyId { get; }
    }
}
