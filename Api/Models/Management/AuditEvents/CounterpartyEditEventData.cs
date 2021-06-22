using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct CounterpartyEditEventData
    {
        public CounterpartyEditEventData(int counterpartyId, CounterpartyEditRequest newCounterpartyInfo)
        {
            CounterpartyId = counterpartyId;
            NewCounterpartyInfo = newCounterpartyInfo;
        }


        public int CounterpartyId { get; }
        public CounterpartyEditRequest NewCounterpartyInfo { get; }
    }
}
