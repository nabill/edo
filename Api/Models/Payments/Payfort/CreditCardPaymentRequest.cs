using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public readonly struct CreditCardPaymentRequest
    {
        [JsonConstructor]
        public CreditCardPaymentRequest(decimal amount, Currencies currency, string token, bool isOneTime, string customerName,
            string customerEmail, string customerIp, string referenceCode, string languageCode, string cardSecurityCode)
        {
            Amount = amount;
            Currency = currency;
            Token = token;
            IsOneTime = isOneTime;
            CustomerEmail = customerEmail;
            CustomerIp = customerIp;
            ReferenceCode = referenceCode;
            LanguageCode = languageCode;
            CardSecurityCode = cardSecurityCode;
            CustomerName = customerName;
        }

        public decimal Amount { get; }
        public Currencies Currency { get; }
        public string Token { get; }
        public bool IsOneTime { get; }
        public string CustomerEmail { get; }
        public string CustomerIp { get; }
        public string ReferenceCode { get; }
        public string LanguageCode { get; }
        public string CardSecurityCode { get; }
        public string CustomerName { get; }
    }
}
