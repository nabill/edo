using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.External
{
    public readonly struct SendPaymentLinkRequest
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
        [Required]
        public string Email { get; }
        
        /// <summary>
        /// Payment data.
        /// </summary>
        [Required]
        public PaymentLinkData PaymentData { get; }
    }
}