using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public readonly struct PaymentData
    {
        public PaymentData(decimal amount, Currencies currency, string reason)
        {
            Amount = amount;
            Currency = currency;
            Reason = reason;
        }
        
        public decimal Amount { get; }
        public Currencies Currency { get; }
        public string Reason { get; }
    }
}