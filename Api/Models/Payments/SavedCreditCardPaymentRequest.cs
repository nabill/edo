using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct SavedCreditCardPaymentRequest
    {
        [JsonConstructor]
        public SavedCreditCardPaymentRequest(int cardId, string referenceCode, string securityCode)
        {
            CardId = cardId;
            ReferenceCode = referenceCode;
            SecurityCode = securityCode;
        }


        /// <summary>
        ///     Id of existing saved credit card
        /// </summary>
        [Required]
        public int CardId { get; }

        /// <summary>
        ///     Service reference code
        /// </summary>
        [Required]
        public string ReferenceCode { get; }

        /// <summary>
        ///     Card security code
        /// </summary>
        [Required]
        public string SecurityCode { get; }
    }
}