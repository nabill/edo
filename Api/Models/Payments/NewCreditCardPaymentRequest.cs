using System.ComponentModel.DataAnnotations;
using HappyTravel.Edo.Api.Models.Payments.CreditCards;
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
            string referenceCode, CreditCardInfo cardInfo, bool isSaveCardNeeded)
        {
            Token = token;
            ReferenceCode = referenceCode;
            CardInfo = cardInfo;
            IsSaveCardNeeded = isSaveCardNeeded;
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

        /// <summary>
        ///     Credit card information
        /// </summary>
        public CreditCardInfo CardInfo { get; }

        /// <summary>
        ///     Flag, indicating that save card is needed, TRUE if card should be saved
        /// </summary>
        public bool IsSaveCardNeeded { get; }
    }
}