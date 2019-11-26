using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Emails;
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


        public Task<Result> SendBillToClient(PaymentBill paymentBill)
        {
            var templateId = string.IsNullOrWhiteSpace(paymentBill.CustomerName)
                ? _options.UnknownCustomerTemplateId
                : _options.KnownCustomerTemplateId;

            return _mailSender.Send(templateId, paymentBill.CustomerEmail, paymentBill);
        }


        private readonly IMailSender _mailSender;
        private readonly PaymentNotificationOptions _options;
    }
}