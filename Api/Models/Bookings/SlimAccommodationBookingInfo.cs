using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Money.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public struct SlimAccommodationBookingInfo
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
            Price = bookingInfo.TotalPrice;
            Currency = bookingInfo.Currency;
            CheckInDate = bookingInfo.CheckInDate;
            CheckOutDate = bookingInfo.CheckOutDate;
            Status = bookingInfo.Status;
            PaymentStatus = bookingInfo.PaymentStatus;
            Rooms = bookingInfo.Rooms;
            Supplier = bookingInfo.Supplier;
        }

        public int Id { get; init; }

        public string ReferenceCode { get; init; }

        public BookingStatuses Status { get; init; }
        public decimal Price { get; init; }

        public Currencies Currency { get; init; }

        public DateTime CheckOutDate { get; init; }

        public DateTime CheckInDate { get; init; }

        public string LocalityName { get; init; }

        public string CountryName { get; init; }

        public string AccommodationName { get; init; }

        public DateTime? Deadline { get; init; }

        public BookingPaymentStatuses PaymentStatus { get; init; }
        
        public List<BookedRoom> Rooms { get; init; }
        
        public Suppliers? Supplier { get; init; }
    }
}