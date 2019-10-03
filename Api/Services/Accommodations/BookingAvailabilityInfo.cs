using System;
using HappyTravel.Edo.Api.Models.Accommodations;

namespace HappyTravel.Edo.Api.Services.Accommodations
{
    public readonly struct BookingAvailabilityInfo
    {
        public BookingAvailabilityInfo(
            string accommodationId, 
            string accommodationName,
            in RichAgreement agreement,
            string cityCode,
            string cityName,
            string countryCode, 
            string countryName, 
            DateTime checkInDate, DateTime checkOutDate)
        {
            AccommodationId = accommodationId;
            AccommodationName = accommodationName;
            Agreement = agreement;
            CityCode = cityCode;
            CityName = cityName;
            CountryCode = countryCode;
            CountryName = countryName;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
        }

        public string AccommodationId { get; }
        public string AccommodationName { get; }
        public RichAgreement Agreement { get; }
        public string CityCode { get; }
        public string CityName { get; }
        public string CountryCode { get; }
        public string CountryName { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }

        public bool Equals(BookingAvailabilityInfo other)
        {
            return (AccommodationId, AccommodationName, Agreement, CityCode, CityName, CountryCode, CountryName, CheckInDate, CheckOutDate)
                .Equals((other.AccommodationId, other.AccommodationName, other.Agreement, other.CityCode, other.CityName, other.CountryCode, other.CountryName, other.CheckInDate, other.CheckOutDate));
        }

        public override bool Equals(object obj)
        {
            return obj is BookingAvailabilityInfo other && Equals(other);
        }
        
        public override int GetHashCode()
            => (AccommodationId, AccommodationName, Agreement, CityCode, CityName, CountryCode, CountryName, CheckInDate, CheckOutDate).GetHashCode();
    }
}