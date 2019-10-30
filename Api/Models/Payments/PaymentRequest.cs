using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    /// <summary>
    ///     Payment request
    /// </summary>
    public readonly struct PaymentRequest
    {
        [JsonConstructor]
        public PaymentRequest(decimal amount, Currencies currency, PaymentTokenInfo token, string referenceCode, string securityCode)
        {
            Amount = amount;
            Currency = currency;
            Token = token;
            ReferenceCode = referenceCode;
            SecurityCode = securityCode;
        }

        /// <summary>
        ///     Payment amount
        /// </summary>
        public decimal Amount { get; }

        /// <summary>
        ///     Currency
        /// </summary>
        public Currencies Currency { get; }

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
