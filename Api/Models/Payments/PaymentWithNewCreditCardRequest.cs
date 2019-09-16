using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct PaymentWithNewCreditCardRequest
    {
        [JsonConstructor]
        public PaymentWithNewCreditCardRequest (decimal amount, Currencies currency, string securityCode, string referenceCode, string number, string expiryDate, string holderName, bool isMemorable)
        {
            Number = number;
            ExpirationDate = expiryDate;
            HolderName = holderName;
            IsMemorable = isMemorable;
            Amount = amount;
            Currency = currency;
            SecurityCode = securityCode;
            ReferenceCode = referenceCode;
        }

        public string Number { get; }
        public string ExpirationDate { get; }
        public string HolderName { get; }
        public bool IsMemorable { get; }
        public decimal Amount { get; }
        public Currencies Currency { get; }
        public string SecurityCode { get; }
        public string ReferenceCode { get; }
    }
}
