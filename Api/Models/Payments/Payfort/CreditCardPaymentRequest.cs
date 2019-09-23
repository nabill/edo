using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments.Payfort
{
    public readonly struct CreditCardPaymentRequest
    {
        [JsonConstructor]
        public CreditCardPaymentRequest(decimal amount, Currencies currency, string cardSecurityCode, string tokenName, bool isMemorable, string customerName,
            string customerEmail, string customerIp, string referenceCode, string language)
        {
            Amount = amount;
            Currency = currency;
            CardSecurityCode = cardSecurityCode;
            TokenName = tokenName;
            IsMemorable = isMemorable;
            CustomerEmail = customerEmail;
            CustomerIp = customerIp;
            ReferenceCode = referenceCode;
            Language = language;
            CustomerName = customerName;
        }

        public decimal Amount { get; }
        public Currencies Currency { get; }
        public string CardSecurityCode { get; }
        public string TokenName { get; }
        public bool IsMemorable { get; }
        public string CustomerEmail { get; }
        public string CustomerIp { get; }
        public string ReferenceCode { get; }
        public string Language { get; }
        public string CustomerName { get; }
    }
}
