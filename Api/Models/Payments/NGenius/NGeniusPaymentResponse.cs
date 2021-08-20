using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct NGeniusPaymentResponse
    {
        public NGeniusPaymentResponse(string paymentId, CreditCardPaymentStatuses status, string merchantOrderReference, ResponsePaymentInformation payment, Secure3dOptions? secure3dOptions = null)
        {
            PaymentId = paymentId;
            Status = status;
            MerchantOrderReference = merchantOrderReference;
            Payment = payment;
            Secure3dOptions = secure3dOptions;
        }

        public string PaymentId { get; }
        public CreditCardPaymentStatuses Status { get; }
        public string MerchantOrderReference { get; }
        public ResponsePaymentInformation Payment { get; }
        public Secure3dOptions? Secure3dOptions { get; }
    }
}