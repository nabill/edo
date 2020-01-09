using System;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct PaymentHistoryData
    {
        public PaymentHistoryData(in DateTime created, decimal amount, object eventData, string currency, int customerId)
        {
            Created = created;
            Amount = amount;
            EventData = eventData;
            Currency = currency;
            CustomerId = customerId;
        }


        /// <summary>
        ///     Current operation date and time
        /// </summary>
        public DateTime Created { get; }

        /// <summary>
        ///     Amount of money for the current operation
        /// </summary>
        public decimal Amount { get; }

        /// <summary>
        ///     Description of the current operation
        /// </summary>
        public object EventData { get; }

        /// <summary>
        ///     Money currency
        /// </summary>
        public string Currency { get; }

        /// <summary>
        ///     Customer Id
        /// </summary>
        public int CustomerId { get; }
    }
}