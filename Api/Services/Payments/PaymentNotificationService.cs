using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Formatters;
using HappyTravel.MailSender;
using Microsoft.Extensions.Options;
using static HappyTravel.Edo.Api.Infrastructure.Formatters.EmailContentFormatter;

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
                amount = FromAmount(paymentBill.Amount, paymentBill.Currency),
                date = $"{paymentBill.Date:u}",
                method = FromEnumDescription(paymentBill.Method),
                referenceCode = paymentBill.ReferenceCode,
                customerName = paymentBill.CustomerName
            };

            return _mailSender.Send(templateId, paymentBill.CustomerEmail, payload);
        }


        public Task<Result> SendNeedPaymentNotificationToCustomer(PaymentBill paymentBill)
        {
            var templateId = _options.NeedPaymentTemplateId;

            var payload = new
            {
                amount = PaymentAmountFormatter.ToCurrencyString(paymentBill.Amount, paymentBill.Currency),
                method = EnumFormatter.ToDescriptionString(paymentBill.Method),
                referenceCode = paymentBill.ReferenceCode,
                customerName = paymentBill.CustomerName
            };

            return _mailSender.Send(templateId, paymentBill.CustomerEmail, payload);
            
        }


        private readonly IMailSender _mailSender;
        private readonly PaymentNotificationOptions _options;
    }
}