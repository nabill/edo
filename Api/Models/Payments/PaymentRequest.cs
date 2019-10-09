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
        public PaymentRequest(decimal amount, Currencies currency, string token, string referenceCode)
        {
            Amount = amount;
            Currency = currency;
            Token = token;
            ReferenceCode = referenceCode;
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
        public string Token { get; }

        /// <summary>
        ///     Booking reference code
        /// </summary>
        public string ReferenceCode { get; }
    }
}
