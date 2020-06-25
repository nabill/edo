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

            return new PaymentLinkData(link.Amount, link.Email, link.ServiceType, link.Currency, link.Comment, link.ReferenceCode, paymentStatus, link.Code);
        }
    }
}