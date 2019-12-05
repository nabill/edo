using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Emails;
using HappyTravel.Edo.Api.Infrastructure.Formatters;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PaymentNotificationService : IPaymentNotificationService
    {
        public PaymentNotificationService(IMailSender mailSender, IOptions<PaymentNotificationOptions> options)
        {
            _mailSender = mailSender;
            _options = options.Value;
        }


        public Task<Result> SendBillToCustomer(PaymentBill paymentBill)
        {
            var templateId = string.IsNullOrWhiteSpace(paymentBill.CustomerName)
                ? _options.UnknownCustomerTemplateId
                : _options.KnownCustomerTemplateId;

            var payload = new
            {
                amount = PaymentAmountFormatter.ToCurrencyString(paymentBill.Amount, paymentBill.Currency),
                date = $"{paymentBill.Date:u}",
                method = EnumFormatter.ToDescriptionString(paymentBill.Method),
                referenceCode = paymentBill.ReferenceCode
            };

            return _mailSender.Send(templateId, paymentBill.CustomerEmail, payload);
        }


        private readonly IMailSender _mailSender;
        private readonly PaymentNotificationOptions _options;
    }
}