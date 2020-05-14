using System;
using System.Collections.Generic;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct AccommodationBookingDetails
    {
        [JsonConstructor]
        public AccommodationBookingDetails(string referenceCode, BookingStatusCodes status,
            DateTime checkInDate, DateTime checkOutDate, AccommodationLocation location,
            string accommodationId, string accommodationName, DateTime? deadlineDate,
            List<BookedRoom> roomDetails)
        {
            ReferenceCode = referenceCode;
            Status = status;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            Location = location;
            AccommodationId = accommodationId;
            AccommodationName = accommodationName;
            DeadlineDate = deadlineDate;
            RoomDetails = roomDetails ?? new List<BookedRoom>(0);
        }
        
        public override bool Equals(object obj) => obj is AccommodationBookingDetails other && Equals(other);


        public bool Equals(AccommodationBookingDetails other)
            => (ReferenceCode, Status, CheckInDate, CheckOutDate, AccommodationId, AccommodationName, DeadlineDate, RoomDetails) ==
                (other.ReferenceCode, other.Status, other.CheckInDate, other.CheckOutDate, other.AccommodationId, other.AccommodationName,
                    other.DeadlineDate, other.RoomDetails);


        public override int GetHashCode()
            => (ReferenceCode, Status, CheckInDate, CheckOutDate, LocationInfo: Location, AccommodationId, AccommodationName, DeadlineDate, RoomDetails).GetHashCode();


        public string ReferenceCode { get; }
        public BookingStatusCodes Status { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public string AccommodationId { get; }
        
        public string AccommodationName { get; }
        
        public AccommodationLocation Location { get; }
        public DateTime? DeadlineDate { get; }
        public List<BookedRoom> RoomDetails { get; }
    }
}