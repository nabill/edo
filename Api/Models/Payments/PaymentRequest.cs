using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct PaymentRequest
    {
        [JsonConstructor]
        public PaymentRequest(decimal amount, Currencies currency, string securityCode, string tokenId, string referenceCode)
        {
            Amount = amount;
            Currency = currency;
            SecurityCode = securityCode;
            TokenId = tokenId;
            ReferenceCode = referenceCode;
        }

        public decimal Amount { get; }
        public Currencies Currency { get; }
        public string SecurityCode { get; }
        public string TokenId { get; }
        public string ReferenceCode { get; }
    }
}
