using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SendGrid.Helpers.Mail;

namespace HappyTravel.Edo.Api.Infrastructure.Email
{
    public interface ITemplatedMailSender
    {
        Task<Result> Send<TMessageData>(string templateId, EmailAddress emailTo, TMessageData messageData);
    }
}