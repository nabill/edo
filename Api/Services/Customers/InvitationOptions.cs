using System;

namespace HappyTravel.Edo.Api.Services.Customers
{
    public class InvitationOptions
    {
        public string MailTemplateId { get; set; }
        public TimeSpan InvitationExpirationPeriod { get; set; }
    }
}