using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.Money.Models;
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
            Price = new MoneyAmount(bookingInfo.TotalPrice, bookingInfo.Currency);
            CheckInDate = bookingInfo.CheckInDate;
            CheckOutDate = bookingInfo.CheckOutDate;
            Status = bookingInfo.Status;
            PaymentStatus = bookingInfo.PaymentStatus;
            Rooms = bookingInfo.Rooms;
            Supplier = bookingInfo.Supplier;
        }
        
        // TODO: replace to readonly struct with init properties after upgrade to C# 9

        public int Id { get; set; }

        public string ReferenceCode { get; set; }

        public BookingStatuses Status { get; set; }

        public MoneyAmount Price { get; set; }

        public DateTime CheckOutDate { get; set; }

        public DateTime CheckInDate { get; set; }

        public string LocalityName { get; set; }

        public string CountryName { get; set; }

        public string AccommodationName { get; set; }

        public DateTime? Deadline { get; set; }

        public BookingPaymentStatuses PaymentStatus { get; set; }
        
        public List<BookedRoom> Rooms { get; set; }
        
        public Suppliers? Supplier { get; set; }
    }
}