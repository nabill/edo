namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct AgentSystemSettingsDeleteEventData
    {
        public AgentSystemSettingsDeleteEventData(int agentId, int agencyId)
        {
            AgentId = agentId;
            AgencyId = agencyId;
        }


        public int AgentId { get; }
        public int AgencyId { get; }
    }
}
