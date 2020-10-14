using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Models;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct AccommodationBookingInfo
    {
        [JsonConstructor]
        public AccommodationBookingInfo(int bookingId, AccommodationBookingDetails bookingDetails, int counterpartyId,
            BookingPaymentStatuses paymentStatus, MoneyAmount totalPrice, DataProviders? dataProvider)
        {
            BookingId = bookingId;
            BookingDetails = bookingDetails;
            CounterpartyId = counterpartyId;
            PaymentStatus = paymentStatus;
            TotalPrice = totalPrice;
            DataProvider = dataProvider;
        }


        public override bool Equals(object obj) => obj is AccommodationBookingInfo other && Equals(other);


        public bool Equals(AccommodationBookingInfo other)
            => Equals((BookingId, BookingDetails, CounterpartyId, PaymentStatus, TotalPrice, DataProvider),
                (other.BookingId, other.BookingDetails, other.CounterpartyId, other.PaymentStatus, TotalPrice, DataProvider));


        public override int GetHashCode() => (BookingId, BookingDetails, CounterpartyId).GetHashCode();


        public int BookingId { get; }
        public AccommodationBookingDetails BookingDetails { get; }
        public int CounterpartyId { get; }
        public BookingPaymentStatuses PaymentStatus { get; }
        public MoneyAmount TotalPrice { get; }
        public DataProviders? DataProvider { get; }
    }
}