using System;
using System.Collections.Generic;
using HappyTravel.EdoContracts.Accommodations.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct AccommodationBookingDetails
    {
        [JsonConstructor]
        public AccommodationBookingDetails(string referenceCode, BookingStatusCodes status,
            DateTime checkInDate, DateTime checkOutDate, string cityCode,
            string accommodationId, string tariffCode, DateTime deadline,
            List<BookingRoomDetailsWithPrice> roomDetails)
        {
            ReferenceCode = referenceCode;
            Status = status;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            CityCode = cityCode;
            AccommodationId = accommodationId;
            TariffCode = tariffCode;
            Deadline = deadline;
            RoomDetails = roomDetails ?? new List<BookingRoomDetailsWithPrice>(0);
        }
        
        public override bool Equals(object obj) => obj is AccommodationBookingDetails other && Equals(other);


        public bool Equals(AccommodationBookingDetails other)
            => (ReferenceCode, Status, CheckInDate, CheckOutDate, CityCode, AccommodationId, TariffCode, Deadline, RoomDetails) ==
                (other.ReferenceCode, other.Status, other.CheckInDate, other.CheckOutDate, other.CityCode, other.AccommodationId, other.TariffCode,
                    other.Deadline, other.RoomDetails);


        public override int GetHashCode()
            => (ReferenceCode, Status, CheckInDate, CheckOutDate, CityCode, AccommodationId, TariffCode, Deadline, RoomDetails).GetHashCode();


        public string ReferenceCode { get; }
        public BookingStatusCodes Status { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public string CityCode { get; }
        public string AccommodationId { get; }
        public string TariffCode { get; }
        public DateTime Deadline { get; }
        public List<BookingRoomDetailsWithPrice> RoomDetails { get; }
    }
}