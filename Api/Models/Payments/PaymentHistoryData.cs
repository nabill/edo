using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct PaymentHistoryData
    {
        public PaymentHistoryData(in DateTime created, decimal amount, object eventData, string currency, int agentId, PaymentHistoryType eventType,
            PaymentMethods paymentMethod)
        {
            Created = created;
            Amount = amount;
            EventData = eventData;
            Currency = currency;
            AgentId = agentId;
            EventType = eventType;
            PaymentMethod = paymentMethod;
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
        ///     Agent Id
        /// </summary>
        public int AgentId { get; }

        /// <summary>
        ///     Current operation type
        /// </summary>
        public PaymentHistoryType EventType { get; }

        /// <summary>
        ///     Payment method
        /// </summary>
        public PaymentMethods PaymentMethod { get; }
    }
}