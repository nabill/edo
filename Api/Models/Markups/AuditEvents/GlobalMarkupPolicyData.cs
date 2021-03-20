using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups.AuditEvents
{
    public readonly struct GlobalMarkupPolicyData
    {
        [JsonConstructor]
        public GlobalMarkupPolicyData(int markupId)
        {
            MarkupId = markupId;
        }


        public int MarkupId { get; }
    }
}