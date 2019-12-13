using SendGrid.Helpers.Mail;

namespace HappyTravel.MailSender.Infrastructure
{
    public class SenderOptions
    {
        public string ApiKey { get; set; }
        public EmailAddress SenderAddress { get; set; }
    }
}
