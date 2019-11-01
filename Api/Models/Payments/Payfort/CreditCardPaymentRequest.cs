using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public readonly struct CreditCardPaymentRequest
    {
        [JsonConstructor]
        public CreditCardPaymentRequest(decimal amount, Currencies currency, PaymentTokenInfo token, string customerName, string customerEmail, string customerIp,
            string referenceCode, string languageCode, bool isNewCard, string securityCode)
        {
            Amount = amount;
            Currency = currency;
            Token = token;
            CustomerEmail = customerEmail;
            CustomerIp = customerIp;
            ReferenceCode = referenceCode;
            LanguageCode = languageCode;
            IsNewCard = isNewCard;
            SecurityCode = securityCode;
            CustomerName = customerName;
        }

        public decimal Amount { get; }
        public Currencies Currency { get; }
        public PaymentTokenInfo Token { get; }
        public string CustomerEmail { get; }
        public string CustomerIp { get; }
        public string ReferenceCode { get; }
        public string LanguageCode { get; }
        public bool IsNewCard { get; }
        
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string SecurityCode { get; }
        
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string CustomerName { get; }
    }
}
