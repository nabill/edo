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
        public PaymentRequest(decimal amount, Currencies currency, string securityCode, string token, string referenceCode, PaymentTokenTypes tokenType)
        {
            Amount = amount;
            Currency = currency;
            SecurityCode = securityCode;
            Token = token;
            ReferenceCode = referenceCode;
            TokenType = tokenType;
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
        ///     Card security code
        /// </summary>
        public string SecurityCode { get; }

        /// <summary>
        ///     Payment token
        /// </summary>
        public string Token { get; }

        /// <summary>
        ///     Booking reference code
        /// </summary>
        public string ReferenceCode { get; }

        /// <summary>
        ///     Payment token type
        /// </summary>
        public PaymentTokenTypes TokenType { get; }
    }
}
