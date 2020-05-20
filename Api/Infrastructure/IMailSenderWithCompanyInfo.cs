using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Mailing;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public interface IMailSenderWithCompanyInfo
    {
        Task<Result> Send(string templateId, string recipientAddress, DataWithCompanyInfo messageData);

        Task<Result> Send(string templateId, IEnumerable<string> recipientAddresses, DataWithCompanyInfo messageData);
    }
}