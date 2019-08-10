using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct HotelBookingDetails
    {
        [JsonConstructor]
        public HotelBookingDetails(string referenceCode, BookingStatusCodes status,
            DateTime checkInDate, DateTime checkOutDate, string cityCode,
            string hotelId, string agreement, int contractTypeId, DateTime deadline, 
            List<BookingRoomDetailsWithPrice> roomDetails)
        {
            ReferenceCode = referenceCode;
            Status = status;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            CityCode = cityCode;
            HotelId = hotelId;
            Agreement = agreement;
            ContractTypeId = contractTypeId;
            Deadline = deadline;
            RoomDetails = roomDetails ?? new List<BookingRoomDetailsWithPrice>(0);
        }
		
        public string ReferenceCode { get; }
        public BookingStatusCodes Status { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public string CityCode { get; }
        public string HotelId { get; }
        public string Agreement { get; }
        public int ContractTypeId { get; }
        public DateTime Deadline { get; }
        public List<BookingRoomDetailsWithPrice> RoomDetails { get; }
    }
}
