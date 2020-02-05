using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    /// <summary>
    ///     Payment request
    /// </summary>
    public readonly struct BookingPaymentRequest
    {
        [JsonConstructor]
        public BookingPaymentRequest(PaymentTokenInfo token, string referenceCode, string securityCode)
        {
            Token = token;
            ReferenceCode = referenceCode;
            SecurityCode = securityCode;
        }


        /// <summary>
        ///     Payment token
        /// </summary>
        public PaymentTokenInfo Token { get; }

        /// <summary>
        ///     Booking reference code
        /// </summary>
        public string ReferenceCode { get; }

        /// <summary>
        ///     Credit card security code
        /// </summary>
        public string SecurityCode { get; }
    }
}