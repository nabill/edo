using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Money.Models;
using HappyTravel.SuppliersCatalog;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    // TODO: NIJO-1226 Create separate model for client api
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
            Supplier = (int) bookingInfo.Supplier;
            Created = bookingInfo.Created;
            HtId = bookingInfo.HtId;
        }

        public int Id { get; init; }

        public string ReferenceCode { get; init; }
        
        public string HtId { get; init; }

        public BookingStatuses Status { get; init; }

        public MoneyAmount Price { get; init; }

        public DateTimeOffset CheckOutDate { get; init; }

        public DateTimeOffset CheckInDate { get; init; }
        
        public DateTimeOffset Created { get; init; }

        public string LocalityName { get; init; }

        public string CountryName { get; init; }

        public string AccommodationName { get; init; }

        public DateTimeOffset? Deadline { get; init; }

        public BookingPaymentStatuses PaymentStatus { get; init; }

        public List<BookedRoom> Rooms { get; init; }
        
        public int? Supplier { get; init; }
    }
}