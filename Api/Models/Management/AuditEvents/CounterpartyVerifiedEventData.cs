namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct CounterpartyVerifiedAuditEventData
    {
        public CounterpartyVerifiedAuditEventData(int counterpartyId, string reason)
        {
            CounterpartyId = counterpartyId;
            Reason = reason;
        }


        public int CounterpartyId { get; }
        public string Reason { get; }
    }
}