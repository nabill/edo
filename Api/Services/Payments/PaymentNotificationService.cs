using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Data.Documents;
using Microsoft.Extensions.Options;
using static HappyTravel.MailSender.Formatters.EmailContentFormatter;
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

            var payload = new PaymentReceiptData
            {
                Date = FormatDate(registrationInfo.Date),
                Number = registrationInfo.Number,
                CustomerName = paymentReceipt.CustomerName,
                Amount = MoneyFormatter.ToCurrencyString(paymentReceipt.Amount, paymentReceipt.Currency),
                Method = FromEnumDescription(paymentReceipt.Method),
                InvoiceNumber = paymentReceipt.InvoiceInfo.Number,
                InvoiceDate = FormatDate(paymentReceipt.InvoiceInfo.Date)
            };

            return _mailSender.Send(templateId, email, payload);
        }

        private static string FormatDate(DateTime date) => date.ToString("dd-MMM-yy");

        private readonly MailSenderWithCompanyInfo _mailSender;
        private readonly PaymentNotificationOptions _options;
    }
}