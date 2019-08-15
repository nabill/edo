using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Hotels;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public struct HotelBookingRequest
    {
        [JsonConstructor]
        public HotelBookingRequest(string searchNumber, string nationality,
            DateTime checkInDate, DateTime checkOutDate, string cityCode, bool availableOnly,
            string hotelId, string agreement, string referenceCode, List<string> responses,
            List<BookingRoomDetails> roomDetails, List<HotelFeature> features)
        {
            SearchNumber = searchNumber;
            Nationality = nationality;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            CityCode = cityCode;
            AvailableOnly = availableOnly;

            HotelId = hotelId;
            Agreement = agreement;

            ReferenceCode = referenceCode;
            Responses = responses ?? new List<string>(0);
            RoomDetails = roomDetails ?? new List<BookingRoomDetails>(0);
            Features = features ?? new List<HotelFeature>(0);
        }

        public string SearchNumber { get; }

        public string Nationality { get; }

        public DateTime CheckInDate { get; }

        public DateTime CheckOutDate { get; }

        public string CityCode { get; }

        public bool AvailableOnly { get; }

        public string HotelId { get; }

        public string Agreement { get; }

        public string ReferenceCode { get; set; }

        public List<string> Responses { get; }

        public List<BookingRoomDetails> RoomDetails { get; }

        public List<HotelFeature> Features { get; }
    }
}
