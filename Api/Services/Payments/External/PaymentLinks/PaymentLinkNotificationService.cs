using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks;
using Microsoft.Extensions.Options;
using MoneyFormatter = HappyTravel.DataFormatters.MoneyFormatter;

namespace HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks
{
    public class PaymentLinkNotificationService : IPaymentLinkNotificationService
    {
        public PaymentLinkNotificationService(IOptions<PaymentLinkOptions> options, MailSenderWithCompanyInfo mailSender)
        {
            _options = options.Value;
            _mailSender = mailSender;
        }


        public Task<Result> SendLink(PaymentLinkData link, string paymentUrl)
        {
            var payload = new PaymentLinkMail
            {
                Amount = MoneyFormatter.ToCurrencyString(link.Amount, link.Currency),
                Comment = link.Comment,
                ServiceDescription = link.ServiceType.ToString(),
                ReferenceCode = link.ReferenceCode,
                PaymentLink = paymentUrl
            };

            return _mailSender.Send(_options.LinkMailTemplateId, link.Email, payload);
        }


        public Task<Result> SendPaymentConfirmation(PaymentLinkData link)
        {
            var payload = new PaymentLinkPaymentConfirmation
            {
                Date = FormatDate(link.Date),
                Amount = MoneyFormatter.ToCurrencyString(link.Amount, link.Currency),
                ReferenceCode = link.ReferenceCode,
                ServiceDescription = link.ServiceType.ToString(),
            };

            return _mailSender.Send(_options.PaymentConfirmationMailTemplateId, link.Email, payload);
        }
        
        
        private static string FormatDate(DateTime date) => date.ToString("dd-MMM-yy");
        
        
        private readonly PaymentLinkOptions _options;
        private readonly MailSenderWithCompanyInfo _mailSender;
    }
}