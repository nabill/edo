using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct AccommodationBookingInfo
    {
        [JsonConstructor]
        public AccommodationBookingInfo(int bookingId, AccommodationBookingDetails bookingDetails, BookingAvailabilityInfo serviceDetails, int counterpartyId,
            BookingPaymentStatuses paymentStatus)
        {
            BookingId = bookingId;
            BookingDetails = bookingDetails;
            ServiceDetails = serviceDetails;
            CounterpartyId = counterpartyId;
            PaymentStatus = paymentStatus;
        }


        public override bool Equals(object obj) => obj is AccommodationBookingInfo other && Equals(other);


        public bool Equals(AccommodationBookingInfo other)
            => Equals((BookingId, BookingDetails, ServiceDetails, CounterpartyId, PaymentStatus),
                (other.BookingId, other.BookingDetails, other.ServiceDetails, other.CounterpartyId, other.PaymentStatus));


        public override int GetHashCode() => (BookingId, BookingDetails, ServiceDetails, CounterpartyId: CounterpartyId).GetHashCode();


        public int BookingId { get; }
        public AccommodationBookingDetails BookingDetails { get; }
        public BookingAvailabilityInfo ServiceDetails { get; }
        public int CounterpartyId { get; }
        public BookingPaymentStatuses PaymentStatus { get; }
    }
}