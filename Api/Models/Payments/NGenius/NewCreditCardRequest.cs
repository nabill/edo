using System.ComponentModel.DataAnnotations;

namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct NewCreditCardRequest
    {
        public NewCreditCardRequest(string referenceCode, Payment card, bool isSaveCardNeeded)
        {
            ReferenceCode = referenceCode;
            Card = card;
            IsSaveCardNeeded = isSaveCardNeeded;
        }


        /// <summary>
        ///     Service reference code
        /// </summary>
        [Required]
        public string ReferenceCode { get; }

        /// <summary>
        ///     Credit card information
        /// </summary>
        public Payment Card { get; }

        /// <summary>
        ///     Flag, indicating that save card is needed, TRUE if card should be saved
        /// </summary>
        public bool IsSaveCardNeeded { get; }
    }
}