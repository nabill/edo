namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct DiscountActivityStateEventData
    {
        public DiscountActivityStateEventData(int agencyId, bool newActivityState)
        {
            AgencyId = agencyId;
            NewActivityState = newActivityState;
        }


        public int AgencyId { get; }

        public bool NewActivityState { get; }
    }
}
