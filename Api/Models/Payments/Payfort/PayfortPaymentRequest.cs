using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public struct PayfortPaymentRequest
    {
        [JsonConstructor]
        public PayfortPaymentRequest(string accessCode, string merchantIdentifier, string merchantReference, string amount, string currency, string language,
            string customerEmail, string customerIp, string tokenName, string customerName, string rememberMe, string settlementReference, string returnUrl,
            string cardSecurityCode)
        {
            AccessCode = accessCode;
            MerchantIdentifier = merchantIdentifier;
            MerchantReference = merchantReference;
            Amount = amount;
            Currency = currency;
            Language = language;
            CustomerEmail = customerEmail;
            CustomerIp = customerIp;
            TokenName = tokenName;
            CustomerName = customerName;
            RememberMe = rememberMe;
            SettlementReference = settlementReference;
            ReturnUrl = returnUrl;
            CardSecurityCode = cardSecurityCode;
            Command = "PURCHASE";
            Signature = string.Empty;
        }

        public string Command { get; }
        public string AccessCode { get; }
        public string MerchantIdentifier { get; }
        public string MerchantReference { get; }
        public string Amount { get; }
        public string Currency { get; }
        public string Language { get; }
        public string CustomerEmail { get; }
        public string CustomerIp { get; }
        public string TokenName { get; }
        public string Signature { get; internal set; }
        public string CustomerName { get; }
        public string RememberMe { get; }
        public string SettlementReference { get; }
        public string ReturnUrl { get; }
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string CardSecurityCode { get; }
    }
}
