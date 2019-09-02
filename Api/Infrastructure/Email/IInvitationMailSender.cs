using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace HappyTravel.Edo.Api.Infrastructure.Email
{
    public interface IInvitationMailSender
    {
        Task SendInvitationEmail(string email, string invitationCode);
    }

    public class InvitationMailSender : IInvitationMailSender
    {
        private readonly InvitationMailOptions _invitationOptions;
        private readonly SenderOptions _senderOptions;

        public InvitationMailSender(IOptions<SenderOptions> senderOptions, IOptions<InvitationMailOptions> invitationOptions)
        {
            _senderOptions = senderOptions.Value;
            _invitationOptions = invitationOptions.Value;
        }
        
        public Task SendInvitationEmail(string email, string invitationCode)
        {
            var client = new SendGridClient(_senderOptions.ApiKey);
            var message = new SendGridMessage
            {
                TemplateId = _invitationOptions.TemplateId,
                From = _senderOptions.SenderAddress
            };
            message.SetTemplateData(new
            {
                InvitationCode = invitationCode
            });

            return client.SendEmailAsync(message);
        }
    }
}