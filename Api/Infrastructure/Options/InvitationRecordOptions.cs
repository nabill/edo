using System;

namespace HappyTravel.Edo.Api.Infrastructure.Options
{
    public class InvitationRecordOptions
    {
        public string AgentInvitationTemplateId { get; set; }
        public string AdminInvitationTemplateId { get; set; }
        public string ChildAgencyInvitationTemplateId { get; set; }
        public string EdoPublicUrl { get; set; }
        public TimeSpan InvitationExpirationPeriod { get; set; }
    }
}