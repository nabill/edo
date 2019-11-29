using System;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct PaymentHistoryData
    {
        public PaymentHistoryData(in DateTime created, decimal amount, object eventData, string currency)
        {
            Created = created;
            Amount = amount;
            EventData = eventData;
            Currency = currency;
        }
        

        public DateTime Created { get; }
        public decimal Amount { get; }
        public object EventData { get; }
        public string Currency { get; }
    }
}
