using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public readonly struct PayfortPaymentRequest
    {
        [JsonConstructor]
        public PayfortPaymentRequest(string accessCode, string merchantIdentifier, string merchantReference, string amount, string currency, string language,
            string agentEmail, string agentIp, string tokenName, string agentName, string settlementReference, string returnUrl, string rememberMe,
            string cardSecurityCode, string signature, string command)
        {
            AccessCode = accessCode;
            MerchantIdentifier = merchantIdentifier;
            MerchantReference = merchantReference;
            Amount = amount;
            Currency = currency;
            Language = language;
            AgentEmail = agentEmail;
            AgentIp = agentIp;
            TokenName = tokenName;
            AgentName = agentName;
            RememberMe = rememberMe;
            SettlementReference = settlementReference;
            ReturnUrl = returnUrl;
            CardSecurityCode = cardSecurityCode;
            Command = command;
            Signature = signature;
        }


        public PayfortPaymentRequest(PayfortPaymentRequest request, string signature) : this(
            signature: signature,
            accessCode: request.AccessCode,
            merchantIdentifier: request.MerchantIdentifier,
            merchantReference: request.MerchantReference,
            amount: request.Amount,
            currency: request.Currency,
            agentName: request.AgentName,
            agentEmail: request.AgentEmail,
            agentIp: request.AgentIp,
            language: request.Language,
            returnUrl: request.ReturnUrl,
            settlementReference: request.SettlementReference,
            tokenName: request.TokenName,
            rememberMe: request.RememberMe,
            cardSecurityCode: request.CardSecurityCode,
            command: request.Command)
        { }


        public string Command { get; }
        public string AccessCode { get; }
        public string MerchantIdentifier { get; }
        public string MerchantReference { get; }
        public string Amount { get; }
        public string Currency { get; }
        public string Language { get; }
        public string AgentEmail { get; }
        public string AgentIp { get; }
        public string TokenName { get; }
        public string Signature { get; }
        public string AgentName { get; }
        public string RememberMe { get; }
        public string SettlementReference { get; }
        public string ReturnUrl { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CardSecurityCode { get; }
    }
}