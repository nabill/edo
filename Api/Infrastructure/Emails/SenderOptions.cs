using SendGrid.Helpers.Mail;

namespace HappyTravel.Edo.Api.Infrastructure.Emails
{
    public class SenderOptions
    {
        public string ApiKey { get; set; }
        public EmailAddress SenderAddress { get; set; }
    }
}