using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Models.Company;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Services.Company;
using HappyTravel.MailSender;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public class MailSenderWithCompanyInfo
    {
        public MailSenderWithCompanyInfo(IMailSender mailSender, ICompanyService companyService)
        {
            _mailSender = mailSender;
            _companyService = companyService;
        }


        public Task<Result> Send(string templateId, string recipientAddress, DataWithCompanyInfo messageData)
        {
            return Validate()
                .Bind(SendEmail);
            
            
            Result Validate()
            {
                return GenericValidator<string>.Validate(v =>
                {
                    v.RuleFor(e => e).NotEmpty().EmailAddress();
                }, recipientAddress);
            }


            Task<Result> SendEmail() 
                => Send(templateId, new[] {recipientAddress}, messageData);
        }


        public async Task<Result> Send(string templateId, IEnumerable<string> recipientAddresses, DataWithCompanyInfo messageData)
        {
            var (_, isFailure, companyInfo, _) = await _companyService.GetCompanyInfo();
            messageData.CompanyInfo = isFailure ? new CompanyInfo() : companyInfo;

            return await _mailSender.Send(templateId, recipientAddresses, messageData);
        }


        private readonly ICompanyService _companyService;
        private readonly IMailSender _mailSender;
    }
}