using HappyTravel.Edo.Api.Models.Payments.CreditCards;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    /// <summary>
    ///     Payment request
    /// </summary>
    public readonly struct NewCreditCardBookingPaymentRequest
    {
        [JsonConstructor]
        public NewCreditCardBookingPaymentRequest(string token,
            string referenceCode, string securityCode, 
            CreditCardInfo cardInfo, bool isSaveCardNeeded)
        {
            Token = token;
            ReferenceCode = referenceCode;
            SecurityCode = securityCode;
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

        /// <summary>
        ///     Credit card security code
        /// </summary>
        public string SecurityCode { get; }

        public CreditCardInfo CardInfo { get; }

        public bool IsSaveCardNeeded { get; }
    }
}