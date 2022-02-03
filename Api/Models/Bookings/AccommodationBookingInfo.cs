using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Models;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct AccommodationBookingInfo
    {
        [JsonConstructor]
        public AccommodationBookingInfo(int bookingId, AccommodationBookingDetails bookingDetails, int agencyId,
            BookingPaymentStatuses paymentStatus, MoneyAmount totalPrice, MoneyAmount cancellationPenalty, int? supplierId,
            BookingAgentInformation agentInformation, PaymentTypes paymentMethod, List<string> tags,
            bool? isDirectContract)
        {
            BookingId = bookingId;
            BookingDetails = bookingDetails;
            AgencyId = agencyId;
            PaymentStatus = paymentStatus;
            TotalPrice = totalPrice;
            CancellationPenalty = cancellationPenalty;
            SupplierId = supplierId;
            AgentInformation = agentInformation;
            PaymentMethod = paymentMethod;
            Tags = tags;
            IsDirectContract = isDirectContract;
        }


        public override bool Equals(object obj) => obj is AccommodationBookingInfo other && Equals(other);


        public bool Equals(AccommodationBookingInfo other)
            => Equals((BookingId, BookingDetails, AgencyId, PaymentStatus, TotalPrice, SupplierId),
                (other.BookingId, other.BookingDetails, other.AgencyId, other.PaymentStatus, TotalPrice, SupplierId));


        public override int GetHashCode() => (BookingId, BookingDetails, AgencyId).GetHashCode();


        public int BookingId { get; }
        public AccommodationBookingDetails BookingDetails { get; }
        public int AgencyId { get; }
        public BookingPaymentStatuses PaymentStatus { get; }
        public MoneyAmount TotalPrice { get; }
        public MoneyAmount CancellationPenalty { get; }
        public int? SupplierId { get; }
        public BookingAgentInformation AgentInformation { get; }
        public PaymentTypes PaymentMethod { get; }
        public List<string> Tags { get; }
        public bool? IsDirectContract { get; }

        public readonly struct BookingAgentInformation
        {
            public BookingAgentInformation(string agentName, string agencyName, string agentEmail)
            {
                AgentName = agentName;
                AgencyName = agencyName;
                AgentEmail = agentEmail;
            }
            public string AgentName { get; }
            public string AgencyName { get; }
            public string AgentEmail { get; }
        }
    }
}