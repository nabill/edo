namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct AgentDeletedEventData
    {
        public AgentDeletedEventData(int agentId)
        {
            AgentId = agentId;
        }
        
        
        public int AgentId { get; }
    }
}