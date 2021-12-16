namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct AgencyDeletionEventData
    {
        public AgencyDeletionEventData(int agencyId)
        {
            AgencyId = agencyId;
        }
        
        public int AgencyId { get; }
    }
}