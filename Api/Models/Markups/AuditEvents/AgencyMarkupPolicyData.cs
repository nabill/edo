using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups.AuditEvents
{
    public readonly struct AgencyMarkupPolicyData
    {
        [JsonConstructor]
        public AgencyMarkupPolicyData(int markupId, int agencyId, decimal toValue)
        {
            MarkupId = markupId;
            AgencyId = agencyId;
            ToValue = toValue;
        }


        public int MarkupId { get; }
        public int AgencyId { get; }
        public decimal ToValue { get; }
    }
}