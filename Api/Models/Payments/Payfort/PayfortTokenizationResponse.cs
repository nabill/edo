using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public readonly struct PayfortTokenizationResponse
    {
        [JsonConstructor]
        public PayfortTokenizationResponse(string serviceCommand, string accessCode, string merchantIdentifier, string merchantReference, string language,
            string expiryDate, string cardNumber, string signature, string tokenName, string responseMessage, string responseCode, string status,
            string cardBin, string cardHolderName, string rememberMe, string returnUrl)
        {
            AccessCode = accessCode;
            MerchantIdentifier = merchantIdentifier;
            MerchantReference = merchantReference;
            Language = language;
            ExpirationDate = expiryDate;
            CardNumber = cardNumber;
            Signature = signature;
            TokenName = tokenName;
            ResponseMessage = responseMessage;
            ResponseCode = responseCode;
            Status = status;
            CardBin = cardBin;
            CardHolderName = cardHolderName;
            RememberMe = rememberMe;
            ReturnUrl = returnUrl;
            ServiceCommand = serviceCommand;
        }

        public string ServiceCommand { get; }
        public string AccessCode { get; }
        public string MerchantIdentifier { get; }
        public string MerchantReference { get; }
        public string Language { get; }
        [JsonProperty("expiry_date")]
        public string ExpirationDate { get; }
        public string CardNumber { get; }
        public string Signature { get; }
        public string TokenName { get; }
        public string ResponseMessage { get; }
        public string ResponseCode { get; }
        public string Status { get; }
        public string CardBin { get; }
        public string CardHolderName { get; }
        public string RememberMe { get; }
        public string ReturnUrl { get; }
    }
}
