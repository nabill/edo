namespace HappyTravel.Edo.Api.Models.Analytics
{
    public readonly struct AgentAnalyticsInfo
    {
        public AgentAnalyticsInfo(string counterpartyName)
        {
            CounterpartyName = counterpartyName;
        }
        
        public string CounterpartyName { get; }
    }
}