using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SendGrid.Helpers.Mail;

namespace HappyTravel.Edo.Api.Infrastructure.Emails
{
    public interface ITemplatedMailSender
    {
        Task<Result> Send<TMessageData>(string templateId, EmailAddress emailTo, TMessageData messageData);
    }
}