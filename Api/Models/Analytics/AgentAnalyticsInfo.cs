namespace HappyTravel.Edo.Api.Models.Analytics
{
    public readonly struct AgentAnalyticsInfo
    {
        public AgentAnalyticsInfo(string agencyName)
        {
            AgencyName = agencyName;
        }
        
        public string AgencyName { get; }
    }
}