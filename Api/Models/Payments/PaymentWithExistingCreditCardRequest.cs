using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct PaymentWithExistingCreditCardRequest
    {
        [JsonConstructor]
        public PaymentWithExistingCreditCardRequest(decimal amount, Currencies currency, string securityCode, string referenceCode, int cardId)
        {
            CardId = cardId;
            Amount = amount;
            Currency = currency;
            SecurityCode = securityCode;
            ReferenceCode = referenceCode;
        }

        public int CardId { get; }
        public decimal Amount { get; }
        public Currencies Currency { get; }
        public string SecurityCode { get; }
        public string ReferenceCode { get; }
    }
}
