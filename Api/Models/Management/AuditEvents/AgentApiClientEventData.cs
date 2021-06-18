namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct AgentApiClientEventData
    {
        public AgentApiClientEventData(int agentId, int agencyId)
        {
            AgentId = agentId;
            AgencyId = agencyId;
        }


        public int AgentId { get; }
        public int AgencyId { get; }
    }
}
