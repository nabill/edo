using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
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

        public decimal Amount { get; }
        public Currencies Currency { get; }
        public string SecurityCode { get; }
        public string Token { get; }
        public string ReferenceCode { get; }
        public PaymentTokenTypes TokenType { get; }
    }
}
