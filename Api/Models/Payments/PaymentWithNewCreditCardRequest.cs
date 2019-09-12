using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct PaymentWithNewCreditCardRequest
    {
        [JsonConstructor]
        public PaymentWithNewCreditCardRequest (decimal amount, Currencies currency, string securityCode, string referenceCode, string number, string expiryDate, string holderName, bool rememberMe)
        {
            Number = number;
            ExpirationDate = expiryDate;
            HolderName = holderName;
            RememberMe = rememberMe;
            Amount = amount;
            Currency = currency;
            SecurityCode = securityCode;
            ReferenceCode = referenceCode;
        }

        public string Number { get; }
        public string ExpirationDate { get; }
        public string HolderName { get; }
        public bool RememberMe { get; }
        public decimal Amount { get; }
        public Currencies Currency { get; }
        public string SecurityCode { get; }
        public string ReferenceCode { get; }
    }
}
