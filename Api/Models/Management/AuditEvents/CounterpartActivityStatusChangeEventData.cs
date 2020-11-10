namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct CounterpartyActivityStatusChangeEventData
    {
        public CounterpartyActivityStatusChangeEventData(int counterpartyId, string reason)
        {
            CounterpartyId = counterpartyId;
            Reason = reason;
        }


        public int CounterpartyId { get; }

        public string Reason { get; }
    }
}