using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups.AuditEvents
{
    public readonly struct CounterpartyMarkupPolicyData
    {
        [JsonConstructor]
        public CounterpartyMarkupPolicyData(int markupId, int counterpartyId)
        {
            MarkupId = markupId;
            CounterpartyId = counterpartyId;
        }


        public int MarkupId { get; }
        public int CounterpartyId { get; }
    }
}