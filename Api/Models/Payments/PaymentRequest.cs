using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct PaymentRequest
    {
        [JsonConstructor]
        public PaymentRequest(decimal amount, Currencies currency, string cardSecurityCode, string tokenId, string referenceCode)
        {
            Amount = amount;
            Currency = currency;
            CardSecurityCode = cardSecurityCode;
            TokenId = tokenId;
            ReferenceCode = referenceCode;
        }

        public decimal Amount { get; }
        public Currencies Currency { get; }
        public string CardSecurityCode { get; }
        public string TokenId { get; }
        public string ReferenceCode { get; }
    }
}
