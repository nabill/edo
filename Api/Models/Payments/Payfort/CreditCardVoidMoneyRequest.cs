using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public readonly struct CreditCardVoidMoneyRequest
    {
        [JsonConstructor]
        public CreditCardVoidMoneyRequest(string externalId, string referenceCode, string languageCode)
        {
            ExternalId = externalId;
            ReferenceCode = referenceCode;
            LanguageCode = languageCode;
        }


        public string ExternalId { get; }
        public string ReferenceCode { get; }
        public string LanguageCode { get; }
    }
}