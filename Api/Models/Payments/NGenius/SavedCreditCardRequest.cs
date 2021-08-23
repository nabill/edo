using System.ComponentModel.DataAnnotations;

namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct SavedCreditCardRequest
    {
        public SavedCreditCardRequest(string referenceCode, int cardId, string cvv)
        {
            ReferenceCode = referenceCode;
            CardId = cardId;
            Cvv = cvv;
        }


        /// <summary>
        ///     Service reference code
        /// </summary>
        [Required]
        public string ReferenceCode { get; }

        /// <summary>
        ///     Saved card id
        /// </summary>
        [Required]
        public int CardId { get; }


        /// <summary>
        ///     CVV code
        /// </summary>
        [Required]
        public string Cvv { get; }
    }
}