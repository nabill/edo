using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public readonly struct PaymentHistoryData
    {
        public PaymentHistoryData(in DateTime created, decimal amount, object eventData, string currency, int agentId, PaymentHistoryType eventType,
            PaymentMethods paymentMethod, string accommodationName, string leadingPassenger, int bookingId, string referenceCode)
        {
            Created = created;
            Amount = amount;
            EventData = eventData;
            Currency = currency;
            AgentId = agentId;
            EventType = eventType;
            PaymentMethod = paymentMethod;
            AccommodationName = accommodationName;
            LeadingPassenger = leadingPassenger;
            BookingId = bookingId;
            ReferenceCode = referenceCode;
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

        /// <summary>
        /// Accommodation title
        /// </summary>
        public string AccommodationName { get; }
        
        /// <summary>
        /// Leading passenger name
        /// </summary>
        public string LeadingPassenger { get; }

        /// <summary>
        /// Id of the booking
        /// </summary>
        public int BookingId { get; }

        /// <summary>
        /// Booking reference code
        /// </summary>
        public string ReferenceCode { get; }
    }
}