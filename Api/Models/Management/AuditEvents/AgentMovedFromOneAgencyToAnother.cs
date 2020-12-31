namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct AgentMovedFromOneAgencyToAnother
    {
        public AgentMovedFromOneAgencyToAnother(int agentId, int sourceAgencyId, int targetAgencyId)
        {
            AgentId = agentId;
            SourceAgencyId = sourceAgencyId;
            TargetAgencyId = targetAgencyId;
        }
        
        
        public int AgentId { get; }
        public int SourceAgencyId { get; }
        public int TargetAgencyId { get; }
    }
}