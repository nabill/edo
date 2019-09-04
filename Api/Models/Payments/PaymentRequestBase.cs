using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public abstract class PaymentRequestBase
    {
        public PaymentRequestBase(decimal amount, Currencies currency, string securityCode)
        {
            Amount = amount;
            Currency = currency;
            SecurityCode = securityCode;
        }

        public decimal Amount { get; }
        public Currencies Currency { get; }
        public string SecurityCode { get; }
    }
}