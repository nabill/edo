using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.MailSender.Formatters;
using Microsoft.Extensions.Options;
using static HappyTravel.MailSender.Formatters.EmailContentFormatter;
using PaymentData = HappyTravel.Edo.Api.Models.Mailing.PaymentData;
using MoneyFormatter = HappyTravel.Money.Helpers.PaymentAmountFormatter;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PaymentNotificationService : IPaymentNotificationService
    {
        public PaymentNotificationService(MailSenderWithCompanyInfo mailSender, IOptions<PaymentNotificationOptions> options)
        {
            _mailSender = mailSender;
            _options = options.Value;
        }


        public Task<Result> SendInvoiceToCustomer(PaymentInvoice paymentInvoice)
        {
            var templateId = string.IsNullOrWhiteSpace(paymentInvoice.CustomerName)
                ? _options.UnknownCustomerTemplateId
                : _options.KnownCustomerTemplateId;

            var payload = new PaymentData
            {
                Amount = MoneyFormatter.ToCurrencyString(paymentInvoice.Amount, paymentInvoice.Currency),
                CustomerName = paymentInvoice.CustomerName,
                Date = FromDateTime(paymentInvoice.Date),
                Method = FromEnumDescription(paymentInvoice.Method),
                ReferenceCode = paymentInvoice.ReferenceCode
            };

            return _mailSender.Send(templateId, paymentInvoice.CustomerEmail, payload);
        }


        public Task<Result> SendNeedPaymentNotificationToCustomer(PaymentInvoice paymentInvoice)
        {
            return _mailSender.Send(_options.NeedPaymentTemplateId, paymentInvoice.CustomerEmail, new PaymentData
            {
                Amount = MoneyFormatter.ToCurrencyString(paymentInvoice.Amount, paymentInvoice.Currency),
                Method = EnumFormatter.ToDescriptionString(paymentInvoice.Method),
                ReferenceCode = paymentInvoice.ReferenceCode,
                CustomerName = paymentInvoice.CustomerName
            });
        }


        private readonly MailSenderWithCompanyInfo _mailSender;
        private readonly PaymentNotificationOptions _options;
    }
}