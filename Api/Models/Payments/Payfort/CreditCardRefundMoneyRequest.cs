using HappyTravel.Money.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public readonly struct CreditCardRefundMoneyRequest
    {
        [JsonConstructor]
        public CreditCardRefundMoneyRequest(decimal amount, Currencies currency, string externalId, string merchantReference, string languageCode)
        {
            Amount = amount;
            Currency = currency;
            ExternalId = externalId;
            MerchantReference = merchantReference;
            LanguageCode = languageCode;
        }


        public decimal Amount { get; }
        public Currencies Currency { get; }
        public string ExternalId { get; }
        public string MerchantReference { get; }
        public string LanguageCode { get; }
    }
}