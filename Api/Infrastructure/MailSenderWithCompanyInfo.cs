using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Models.Company;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Messaging;
using HappyTravel.Edo.Api.Services.Company;
using HappyTravel.Edo.Api.Services.Messaging;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public class MailSenderWithCompanyInfo
    {
        public MailSenderWithCompanyInfo(IMessageBus messageBus, ICompanyService companyService)
        {
            _messageBus = messageBus;
            _companyService = companyService;
        }


        public async Task Send(string templateId,
            IEnumerable<string> recipientAddresses,
            DataWithCompanyInfo messageData,
            List<MailAttachment>? attachments = default)
        {
            var (_, isFailure, companyInfo, _) = await _companyService.GetCompanyInfo();
            messageData.CompanyInfo = isFailure ? new CompanyInfo() : companyInfo;

            _messageBus.Publish(MessageBusTopics.SendMail, new MailMessage
            {
                TemplateId = templateId,
                Recipients = recipientAddresses,
                Data = messageData,
                Attachments = attachments
            });
        }


        private readonly ICompanyService _companyService;
        private readonly IMessageBus _messageBus;
    }
}