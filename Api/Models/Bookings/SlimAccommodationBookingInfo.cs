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
            Id = bookingInfo.Id;
            ReferenceCode = bookingInfo.ReferenceCode;
            AccommodationName = bookingInfo.AccommodationName;
            CountryName = bookingInfo.Location.Country;
            LocalityName = bookingInfo.Location.Locality;
            Deadline = bookingInfo.DeadlineDate;
            Price = new MoneyAmount(bookingInfo.TotalPrice, bookingInfo.Currency);
            CheckInDate = bookingInfo.CheckInDate;
            CheckOutDate = bookingInfo.CheckOutDate;
            Status = bookingInfo.Status;
            PaymentStatus = bookingInfo.PaymentStatus;
            Rooms = bookingInfo.Rooms;
        }
        
        
        public int Id { get; }

        public string ReferenceCode { get; }

        public BookingStatuses Status { get; }

        public MoneyAmount Price { get; }

        public DateTime CheckOutDate { get; }

        public DateTime CheckInDate { get; }

        public string LocalityName { get; }

        public string CountryName { get; }

        public string AccommodationName { get; }

        public DateTime? Deadline { get; }

        public BookingPaymentStatuses PaymentStatus { get; }
        
        public List<BookedRoom> Rooms { get; }
    }
}