using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Management.AuditEvents
{
    public readonly struct AgencyVerifiedEventData
    {
        public AgencyVerifiedEventData(int agencyId, string reason, AgencyVerificationStates verificationState)
        {
            AgencyId = agencyId;
            Reason = reason;
            VerificationState = verificationState;
        }


        public int AgencyId { get; }
        public string Reason { get; }
        public AgencyVerificationStates VerificationState { get; }
    }
}