namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct AgencyActivityStatusChangeEventData
    {
        public AgencyActivityStatusChangeEventData(int agencyId, string reason)
        {
            AgencyId = agencyId;
            Reason = reason;
        }
        
        public int AgencyId { get; }
        public string Reason { get; }
    }
}