using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Data.Documents;
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


        public Task<Result> SendReceiptToCustomer((DocumentRegistrationInfo RegistrationInfo, PaymentReceipt Data) receipt, string email)
        {
            var (registrationInfo, paymentReceipt) = receipt;
            var templateId = string.IsNullOrWhiteSpace(paymentReceipt.CustomerName)
                ? _options.UnknownCustomerTemplateId
                : _options.KnownCustomerTemplateId;

            var payload = new PaymentData
            {
                Amount = MoneyFormatter.ToCurrencyString(paymentReceipt.Amount, paymentReceipt.Currency),
                CustomerName = paymentReceipt.CustomerName,
                Date = FromDateTime(registrationInfo.Date),
                Method = FromEnumDescription(paymentReceipt.Method),
                ReferenceCode = paymentReceipt.ReferenceCode
            };

            return _mailSender.Send(templateId, email, payload);
        }


        private readonly MailSenderWithCompanyInfo _mailSender;
        private readonly PaymentNotificationOptions _options;
    }
}