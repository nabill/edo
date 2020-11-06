namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct CounterpartActivityStatusChangeEventData
    {
        public CounterpartActivityStatusChangeEventData(int counterpartyId, string reason)
        {
            CounterpartyId = counterpartyId;
            Reason = reason;
        }


        public int CounterpartyId { get; }

        public string Reason { get; }
    }
}