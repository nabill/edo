using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct PaymentReceipt
    {
        public PaymentReceipt(decimal amount, Currencies currency, 
            PaymentMethods method, string referenceCode, string clientName = default)
        {
            Amount = amount;
            Currency = currency;
            Method = method;
            ReferenceCode = referenceCode;
            CustomerName = clientName ?? string.Empty;
        }


        public decimal Amount { get; }
        public Currencies Currency { get; }
        public PaymentMethods Method { get; }
        public string ReferenceCode { get; }
        public string CustomerName { get; }
    }
}