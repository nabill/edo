using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Models;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct AccommodationBookingInfo
    {
        [JsonConstructor]
        public AccommodationBookingInfo(int bookingId, AccommodationBookingDetails bookingDetails, int counterpartyId,
            BookingPaymentStatuses paymentStatus, MoneyAmount totalPrice, Suppliers? supplier,
            BookingAgentInformation agentInformation, PaymentMethods paymentMethod)
        {
            BookingId = bookingId;
            BookingDetails = bookingDetails;
            CounterpartyId = counterpartyId;
            PaymentStatus = paymentStatus;
            TotalPrice = totalPrice;
            Supplier = supplier;
            AgentInformation = agentInformation;
            PaymentMethod = paymentMethod;
        }


        public override bool Equals(object obj) => obj is AccommodationBookingInfo other && Equals(other);


        public bool Equals(AccommodationBookingInfo other)
            => Equals((BookingId, BookingDetails, CounterpartyId, PaymentStatus, TotalPrice, Supplier),
                (other.BookingId, other.BookingDetails, other.CounterpartyId, other.PaymentStatus, TotalPrice, Supplier));


        public override int GetHashCode() => (BookingId, BookingDetails, CounterpartyId).GetHashCode();


        public int BookingId { get; }
        public AccommodationBookingDetails BookingDetails { get; }
        public int CounterpartyId { get; }
        public BookingPaymentStatuses PaymentStatus { get; }
        public MoneyAmount TotalPrice { get; }
        public Suppliers? Supplier { get; }
        public BookingAgentInformation AgentInformation { get; }
        public PaymentMethods PaymentMethod { get; }

        public readonly struct BookingAgentInformation
        {
            public BookingAgentInformation(string agentName, string agencyName, string counterpartyName, string agentEmail)
            {
                AgentName = agentName;
                AgencyName = agencyName;
                CounterpartyName = counterpartyName;
                AgentEmail = agentEmail;
            }
            public string AgentName { get; }
            public string AgencyName { get; }
            public string CounterpartyName { get; }
            public string AgentEmail { get; }
        }
    }
}