using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    /// <summary>
    ///     Payment request
    /// </summary>
    public readonly struct NewCreditCardPaymentRequest
    {
        [JsonConstructor]
        public NewCreditCardPaymentRequest(string token,
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