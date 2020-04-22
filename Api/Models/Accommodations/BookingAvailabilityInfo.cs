using System;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Accommodations
{
    public readonly struct BookingAvailabilityInfo
    {
        [JsonConstructor]
        public BookingAvailabilityInfo(
            string accommodationId,
            string accommodationName,
            in RoomContractSet roomContractSet,
            string cityCode,
            string cityName,
            string countryCode,
            string countryName,
            DateTime checkInDate,
            DateTime checkOutDate)
        {
            AccommodationId = accommodationId;
            AccommodationName = accommodationName;
            RoomContractSet = roomContractSet;
            CityCode = cityCode;
            CityName = cityName;
            CountryCode = countryCode;
            CountryName = countryName;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
        }


        private BookingAvailabilityInfo(
            string accommodationId,
            string accommodationName,
            in RoomContractSet roomContractSet,
            DeadlineDetails deadlineDetails,
            string cityCode,
            string cityName,
            string countryCode,
            string countryName,
            DateTime checkInDate,
            DateTime checkOutDate)
        {
            AccommodationId = accommodationId;
            AccommodationName = accommodationName;
            RoomContractSet = roomContractSet;
            CityCode = cityCode;
            CityName = cityName;
            CountryCode = countryCode;
            CountryName = countryName;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
        }


        public BookingAvailabilityInfo AddDeadlineDetails(DeadlineDetails deadlineDetails)
            => new BookingAvailabilityInfo(AccommodationId, AccommodationName, RoomContractSet, deadlineDetails, CityCode, CityName, CountryCode, CountryName,
                CheckInDate, CheckOutDate);


        public string AccommodationId { get; }
        public string AccommodationName { get; }
        public RoomContractSet RoomContractSet { get; }
        public string CityCode { get; }
        public string CityName { get; }
        public string CountryCode { get; }
        public string CountryName { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }


        public bool Equals(BookingAvailabilityInfo other)
            => (AccommodationId, AccommodationName, RoomContractSet: RoomContractSet, CityCode, CityName, CountryCode, CountryName, CheckInDate, CheckOutDate)
                .Equals((other.AccommodationId, other.AccommodationName, other.RoomContractSet, other.CityCode, other.CityName,
                    other.CountryCode, other.CountryName, other.CheckInDate, other.CheckOutDate));


        public override bool Equals(object obj) => obj is BookingAvailabilityInfo other && Equals(other);


        public override int GetHashCode()
            => (AccommodationId, AccommodationName, RoomContractSet: RoomContractSet, CityCode, CityName, CountryCode, CountryName, CheckInDate, CheckOutDate)
                .GetHashCode();
    }
}