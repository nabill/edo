using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.External
{
    public struct SendPaymentLinkRequest
    {
        [JsonConstructor]
        public SendPaymentLinkRequest(string email, PaymentLinkData paymentData)
        {
            Email = email;
            PaymentData = paymentData;
        }
        
        /// <summary>
        /// E-mail to send link.
        /// </summary>
        public string Email { get; }
        
        /// <summary>
        /// Payment data.
        /// </summary>
        public PaymentLinkData PaymentData { get; }
    }
}