using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.PaymentLinks;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks
{
    public static class PaymentLinkExtensions
    {
        public static PaymentLinkData ToLinkData(this PaymentLink link)
        {
            var paymentStatus = string.IsNullOrWhiteSpace(link.LastPaymentResponse)
                ? CreditCardPaymentStatuses.Created
                : JsonConvert.DeserializeObject<PaymentResponse>(link.LastPaymentResponse).Status;

            return new PaymentLinkData(amount: link.Amount,
                email: link.Email,
                serviceType: link.ServiceType,
                currency: link.Currency,
                comment: link.Comment, 
                referenceCode: link.ReferenceCode,
                creditCardPaymentStatus: paymentStatus,
                code: link.Code,
                date: link.Created,
                paymentProcessor: link.PaymentProcessor);
        }
    }
}