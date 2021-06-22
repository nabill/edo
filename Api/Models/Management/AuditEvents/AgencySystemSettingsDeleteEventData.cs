namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct AgencySystemSettingsDeleteEventData
    {
        public AgencySystemSettingsDeleteEventData(int agencyId)
        {
            AgencyId = agencyId;
        }


        public int AgencyId { get; }
    }
}
