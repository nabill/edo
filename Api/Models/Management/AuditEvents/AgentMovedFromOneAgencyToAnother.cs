namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct AgentMovedFromOneAgencyToAnother
    {
        public AgentMovedFromOneAgencyToAnother(int agentId, int sourceAgencyId, int destinationAgencyId)
        {
            AgentId = agentId;
            SourceAgencyId = sourceAgencyId;
            DestinationAgencyId = destinationAgencyId;
        }
        
        
        public int AgentId { get; }
        public int SourceAgencyId { get; }
        public int DestinationAgencyId { get; }
    }
}