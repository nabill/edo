using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.Money.Models;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct SlimAccommodationBookingInfo
    {
        [JsonConstructor]
        public SlimAccommodationBookingInfo(Booking bookingInfo)
        {
            var serviceDetails = JsonConvert.DeserializeObject<BookingAvailabilityInfo>(bookingInfo.ServiceDetails);
            Id = bookingInfo.Id;
            ReferenceCode = bookingInfo.ReferenceCode;
            AccommodationName = serviceDetails.AccommodationName;
            CountryName = serviceDetails.CountryName;
            LocalityName = serviceDetails.LocalityName;
            Deadline = bookingInfo.DeadlineDate;
            Price = new MoneyAmount(bookingInfo.TotalPrice, bookingInfo.Currency);
            CheckInDate = bookingInfo.CheckInDate;
            CheckOutDate = bookingInfo.CheckOutDate;
            Status = bookingInfo.Status;
            PaymentStatus = bookingInfo.PaymentStatus;
            SlimRoomContracts = serviceDetails.RoomContractSet.RoomContracts != null
                ? serviceDetails.RoomContractSet.RoomContracts.Select(i => new SlimRoomContract(i)).ToList()
                : new List<SlimRoomContract>(0);
        }
        
        
        public int Id { get; }

        public string ReferenceCode { get; }

        public BookingStatusCodes Status { get; }

        public MoneyAmount Price { get; }

        public DateTime CheckOutDate { get; }

        public DateTime CheckInDate { get; }

        public string LocalityName { get; }

        public string CountryName { get; }

        public string AccommodationName { get; }

        public DateTime? Deadline { get; }

        public BookingPaymentStatuses PaymentStatus { get; }
        
        public List<SlimRoomContract> SlimRoomContracts { get; }
    }
}