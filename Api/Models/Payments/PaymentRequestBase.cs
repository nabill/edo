using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public abstract class PaymentRequestBase
    {
        public PaymentRequestBase(decimal amount, Currencies currency, string securityCode, string referenceCode)
        {
            Amount = amount;
            Currency = currency;
            SecurityCode = securityCode;
            ReferenceCode = referenceCode;
        }

        public decimal Amount { get; }
        public Currencies Currency { get; }
        public string SecurityCode { get; }
        public string ReferenceCode { get; }
    }
}
