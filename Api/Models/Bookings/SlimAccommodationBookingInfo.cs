using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct SlimAccommodationBookingInfo
    {
        [JsonConstructor]
        public SlimAccommodationBookingInfo(Booking bookingInfo)
        {
            var serviceDetails = JsonConvert.DeserializeObject<BookingAvailabilityInfo>(bookingInfo.ServiceDetails);
            var bookingDetails = JsonConvert.DeserializeObject<BookingDetails>(bookingInfo.BookingDetails);
            Id = bookingInfo.Id;
            ReferenceCode = bookingDetails.ReferenceCode;
            AccommodationName = serviceDetails.AccommodationName;
            CountryName = serviceDetails.CountryName;
            LocalityName = serviceDetails.CityName;
            Deadline = bookingDetails.Deadline;
            DeadlineDetails = serviceDetails.DeadlineDetails;
            Price = serviceDetails.RoomContractSet.Price;
            CheckInDate = bookingDetails.CheckInDate;
            CheckOutDate = bookingDetails.CheckOutDate;
            Status = bookingDetails.Status;
            PaymentStatus = bookingInfo.PaymentStatus;
            RoomContractInfo = serviceDetails.RoomContractSet.RoomContracts != null
                ? serviceDetails.RoomContractSet.RoomContracts.Select(i => new SlimRoomContractInfo(i)).ToList()
                : new List<SlimRoomContractInfo>(0);
        }
        
        
        public int Id { get; }

        public string ReferenceCode { get; }

        public BookingStatusCodes Status { get; }

        public Price Price { get; }

        public DateTime CheckOutDate { get; }

        public DateTime CheckInDate { get; }

        public string LocalityName { get; }

        public string CountryName { get; }

        public string AccommodationName { get; }

        public DateTime Deadline { get; }

        public DeadlineDetails DeadlineDetails { get; }

        public BookingPaymentStatuses PaymentStatus { get; }
        
        public List<SlimRoomContractInfo> RoomContractInfo { get; }
    }
}