using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    /// <summary>
    ///     Payment request
    /// </summary>
    public readonly struct PaymentRequest
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="amount">Payment amount</param>
        /// <param name="currency">Currency</param>
        /// <param name="securityCode">Card security code</param>
        /// <param name="token">Payment token</param>
        /// <param name="referenceCode">Booking reference code</param>
        /// <param name="tokenType">Payment token type</param>
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
