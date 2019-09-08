using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SendGrid.Helpers.Mail;

namespace HappyTravel.Edo.Api.Infrastructure.Emails
{
    public interface IMailSender
    {
        Task<Result> Send<TMessageData>(string templateId, string recipientAddress, TMessageData messageData);
    }
}