using System;
using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public readonly struct BookingAvailabilityInfo
    {
        public BookingAvailabilityInfo(string accommodationId,
            in RichAgreement agreement, string countryCode, 
            DateTime checkInDate, DateTime checkOutDate)
        {
            AccommodationId = accommodationId;
            Agreement = agreement;
            CountryCode = countryCode;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
        }
        public string AccommodationId { get; }
        public RichAgreement Agreement { get; }
        public string CountryCode { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }

        public bool Equals(BookingAvailabilityInfo other)
        {
            return (AccommodationId, Agreement, CountryCode, CheckInDate, CheckOutDate)
                .Equals((other.AccommodationId, other.Agreement, other.CountryCode, other.CheckInDate, other.CheckOutDate));
        }

        public override bool Equals(object obj)
        {
            return obj is BookingAvailabilityInfo other && Equals(other);
        }

        public override int GetHashCode() => throw new NotSupportedException();
    }
}