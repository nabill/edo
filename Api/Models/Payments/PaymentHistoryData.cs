using System;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Models.Payments
{
    public struct PaymentHistoryData
    {
        public PaymentHistoryData(in DateTime created, decimal amount, object eventData, Currencies currency, int agentId, PaymentHistoryType eventType,
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
        public DateTime Created { get; set; }

        /// <summary>
        ///     Amount of money for the current operation
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        ///     Description of the current operation
        /// </summary>
        public object EventData { get; set; }

        /// <summary>
        ///     Money currency
        /// </summary>
        public Currencies Currency { get; set; }

        /// <summary>
        ///     Agent Id
        /// </summary>
        public int AgentId { get; set; }

        /// <summary>
        ///     Current operation type
        /// </summary>
        public PaymentHistoryType EventType { get; set; }

        /// <summary>
        ///     Payment method
        /// </summary>
        public PaymentMethods PaymentMethod { get; set; }

        /// <summary>
        /// Accommodation title
        /// </summary>
        public string AccommodationName { get; set; }
        
        /// <summary>
        /// Leading passenger name
        /// </summary>
        public string LeadingPassenger { get; set; }

        /// <summary>
        /// Id of the booking
        /// </summary>
        public int BookingId { get; set; }

        /// <summary>
        /// Booking reference code
        /// </summary>
        public string ReferenceCode { get; set; }
    }
}