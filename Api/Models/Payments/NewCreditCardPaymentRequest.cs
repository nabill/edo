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
        public string Token { get; }

        /// <summary>
        ///     Booking reference code
        /// </summary>
        public string ReferenceCode { get; }

        public CreditCardInfo CardInfo { get; }

        public bool IsSaveCardNeeded { get; }
    }
}