namespace HappyTravel.Edo.Api.Models.Management
{
    public readonly struct ChangeAgentAgencyRequest
    {
        public ChangeAgentAgencyRequest(int agentId, int sourceAgencyId, int destinationAgencyId)
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