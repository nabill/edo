using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.MailSender.Formatters;
using Microsoft.Extensions.Options;
using static HappyTravel.MailSender.Formatters.EmailContentFormatter;
using PaymentData = HappyTravel.Edo.Api.Models.Mailing.PaymentData;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PaymentNotificationService : IPaymentNotificationService
    {
        public PaymentNotificationService(MailSenderWithCompanyInfo mailSender, IOptions<PaymentNotificationOptions> options)
        {
            _mailSender = mailSender;
            _options = options.Value;
        }


        public Task<Result> SendBillToCustomer(PaymentBill paymentBill)
        {
            var templateId = string.IsNullOrWhiteSpace(paymentBill.CustomerName)
                ? _options.UnknownCustomerTemplateId
                : _options.KnownCustomerTemplateId;

            var payload = new PaymentData
            {
                Amount = FromAmount(paymentBill.Amount, paymentBill.Currency),
                CustomerName = paymentBill.CustomerName,
                Date = FromDateTime(paymentBill.Date),
                Method = FromEnumDescription(paymentBill.Method),
                ReferenceCode = paymentBill.ReferenceCode
            };

            return _mailSender.Send(templateId, paymentBill.CustomerEmail, payload);
        }


        public Task<Result> SendNeedPaymentNotificationToCustomer(PaymentBill paymentBill)
        {
            return _mailSender.Send(_options.NeedPaymentTemplateId, paymentBill.CustomerEmail, new PaymentData
            {
                Amount = PaymentAmountFormatter.ToCurrencyString(paymentBill.Amount, paymentBill.Currency),
                Method = EnumFormatter.ToDescriptionString(paymentBill.Method),
                ReferenceCode = paymentBill.ReferenceCode,
                CustomerName = paymentBill.CustomerName
            });
        }


        private readonly MailSenderWithCompanyInfo _mailSender;
        private readonly PaymentNotificationOptions _options;
    }
}