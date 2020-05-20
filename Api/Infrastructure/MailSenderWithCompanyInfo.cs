using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Company;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Services.Company;
using HappyTravel.MailSender;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public class MailSenderWithCompanyInfo: IMailSenderWithCompanyInfo
    {
        public MailSenderWithCompanyInfo(IMailSender mailSender, ICompanyService companyService)
        {
            _mailSender = mailSender;
            _companyService = companyService;
        }


        public Task<Result> Send(string templateId, string recipientAddress, DataWithCompanyInfo messageData)
            => Send(templateId, new[] {recipientAddress}, messageData);


        public async Task<Result> Send(string templateId, IEnumerable<string> recipientAddresses, DataWithCompanyInfo messageData)
        {
            var (_, isFailure, companyInfo, error) = await _companyService.Get();
            messageData.CompanyInfo = isFailure ? new CompanyInfo() : companyInfo;
            return await _mailSender.Send(templateId, recipientAddresses, messageData);
        }


        private readonly ICompanyService _companyService;
        private readonly IMailSender _mailSender;
    }
}