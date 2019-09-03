using System.Threading.Tasks;
using SendGrid.Helpers.Mail;

namespace HappyTravel.Edo.Api.Infrastructure.Email
{
    public interface ITemplatedMailSender
    {
        Task Send<TMessageData>(string templateId, EmailAddress emailTo, TMessageData messageData);
    }
}