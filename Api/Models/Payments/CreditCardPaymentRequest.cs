using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    /// <summary>
    ///     Payment request
    /// </summary>
    public readonly struct CreditCardPaymentRequest
    {
        [JsonConstructor]
        public CreditCardPaymentRequest(string token,
            string referenceCode)
        {
            Token = token;
            ReferenceCode = referenceCode;
        }


        /// <summary>
        ///     Payment token
        /// </summary>
        [Required]
        public string Token { get; }

        /// <summary>
        ///     Service reference code
        /// </summary>
        [Required]
        public string ReferenceCode { get; }
    }
}