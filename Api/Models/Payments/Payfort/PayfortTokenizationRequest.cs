using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public struct PayfortTokenizationRequest
    {
        [JsonConstructor]
        public PayfortTokenizationRequest( string accessCode, string merchantIdentifier, string merchantReference, string language,
            string expiryDate, string cardNumber, string cardSecurityCode, string cardHolderName, string rememberMe, string returnUrl)
        {
            ServiceCommand = "TOKENIZATION";
            AccessCode = accessCode;
            MerchantIdentifier = merchantIdentifier;
            MerchantReference = merchantReference;
            Language = language;
            ExpiryDate = expiryDate;
            CardNumber = cardNumber;
            CardSecurityCode = cardSecurityCode;
            CardHolderName = cardHolderName;
            RememberMe = rememberMe;
            ReturnUrl = returnUrl;
            Signature = string.Empty;
        }

        public string ServiceCommand { get; }
        public string AccessCode { get; }
        public string MerchantIdentifier { get; }
        public string MerchantReference { get; }
        public string Language { get; }
        public string ExpiryDate { get; }
        public string CardNumber { get; }
        public string CardSecurityCode { get; }
        public string CardHolderName { get; }
        public string RememberMe { get; }
        public string ReturnUrl { get; }
        public string Signature { get; internal set; }

    }
}
