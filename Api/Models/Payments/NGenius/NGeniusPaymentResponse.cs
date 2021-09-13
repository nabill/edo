using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct NGeniusPaymentResponse
    {
        public NGeniusPaymentResponse(string paymentId, string orderReference, string merchantOrderReference, string paymentLink)
        {
            PaymentId = paymentId;
            OrderReference = orderReference;
            MerchantOrderReference = merchantOrderReference;
            PaymentLink = paymentLink;
        }

        public string PaymentId { get; }
        public string OrderReference { get; }
        public string MerchantOrderReference { get; }
        public string PaymentLink { get; }
    }
}