using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.MailSender;
using HappyTravel.MailSender.Formatters;
using Microsoft.Extensions.Options;
using static HappyTravel.MailSender.Formatters.EmailContentFormatter;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PaymentNotificationService : IPaymentNotificationService
    {
        public PaymentNotificationService(IMailSender mailSender, IOptions<PaymentNotificationOptions> options)
        {
            _mailSender = mailSender;
            _options = options.Value;
        }


        public Task<Result> SendBillToAgent(PaymentBill paymentBill)
        {
            var templateId = string.IsNullOrWhiteSpace(paymentBill.AgentName)
                ? _options.UnknownAgentTemplateId
                : _options.KnownAgentTemplateId;

            var payload = new
            {
                amount = FromAmount(paymentBill.Amount, paymentBill.Currency),
                agentName = paymentBill.AgentName,
                date = FromDateTime(paymentBill.Date),
                method = FromEnumDescription(paymentBill.Method),
                referenceCode = paymentBill.ReferenceCode
            };

            return _mailSender.Send(templateId, paymentBill.AgentEmail, payload);
        }


        public Task<Result> SendNeedPaymentNotificationToAgent(PaymentBill paymentBill)
            => _mailSender.Send(_options.NeedPaymentTemplateId, paymentBill.AgentEmail, new
            {
                amount = PaymentAmountFormatter.ToCurrencyString(paymentBill.Amount, paymentBill.Currency),
                method = EnumFormatter.ToDescriptionString(paymentBill.Method),
                referenceCode = paymentBill.ReferenceCode,
                agentName = paymentBill.AgentName
            });


        private readonly IMailSender _mailSender;
        private readonly PaymentNotificationOptions _options;
    }
}