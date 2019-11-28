using HappyTravel.Edo.Api.Services.Accommodations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct AccommodationBookingInfoResponse
    {
        [JsonConstructor]
        public AccommodationBookingInfoResponse(int bookingId, AccommodationBookingDetails bookingDetails, BookingAvailabilityInfo serviceDetails,
            int companyId)
        {
            BookingId = bookingId;
            BookingDetails = bookingDetails;
            ServiceDetails = serviceDetails;
            CompanyId = companyId;
        }


        public override bool Equals(object obj) => obj is AccommodationBookingInfoResponse other && Equals(other);


        public bool Equals(AccommodationBookingInfoResponse other)
            => Equals((BookingId, BookingDetails, ServiceDetails, CompanyId),
                (other.BookingId, other.BookingDetails, other.ServiceDetails, other.CompanyId));


        public override int GetHashCode() => (BookingId, BookingDetails, ServiceDetails, CompanyId).GetHashCode();


        public int BookingId { get; }
        public AccommodationBookingDetails BookingDetails { get; }
        public BookingAvailabilityInfo ServiceDetails { get; }
        public int CompanyId { get; }
    }
}