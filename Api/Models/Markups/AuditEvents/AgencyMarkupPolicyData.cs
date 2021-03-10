using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups.AuditEvents
{
    public readonly struct AgencyMarkupPolicyData
    {
        [JsonConstructor]
        public AgencyMarkupPolicyData(int markupId, int agencyId)
        {
            MarkupId = markupId;
            AgencyId = agencyId;
        }


        public int MarkupId { get; }
        public int AgencyId { get; }
    }
}