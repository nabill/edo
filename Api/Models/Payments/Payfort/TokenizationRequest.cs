using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public readonly struct TokenizationRequest
    {
        [JsonConstructor]
        public TokenizationRequest(string cardNumber, string cardHolderName, string cardSecurityCode, string expiryDate, string referenceCode, bool rememberMe, string language)
        {
            CardNumber = cardNumber;
            CardHolderName = cardHolderName;
            CardSecurityCode = cardSecurityCode;
            ExpirationDate = expiryDate;
            ReferenceCode = referenceCode;
            RememberMe = rememberMe;
            Language = language;
        }

        public string CardNumber { get; }
        public string CardHolderName { get; }
        public string CardSecurityCode { get; }
        public string ExpirationDate { get; }
        public string ReferenceCode { get; }
        public bool RememberMe { get; }
        public string Language { get; }
    }
}
