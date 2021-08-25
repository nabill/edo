namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct AdministratorChangeActivityStateEventData
    {
        public int AdministratorId { get; init; }
        public bool NewActivityState { get; init; }
    }
}
