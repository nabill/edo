using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups.AuditEvents
{
    public readonly struct GlobalMarkupPolicyData
    {
        [JsonConstructor]
        public GlobalMarkupPolicyData(int markupId, decimal toValue)
        {
            MarkupId = markupId;
            ToValue = toValue;
        }


        public int MarkupId { get; }
        public decimal ToValue { get; }
    }
}