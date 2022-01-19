using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.DirectApi.Models.Booking
{
    public readonly struct Booking
    {
        public Booking(string clientReferenceCode, string referenceCode, DateTime created, DateTime checkInDate, DateTime checkOutDate, MoneyAmount totalPrice,
            BookingStatuses status, List<BookedRoom> rooms, string accommodationId, List<CancellationPolicy> cancellationPolicies, DateTime? cancelled,
            bool isPackage)
        {
            ClientReferenceCode = clientReferenceCode;
            ReferenceCode = referenceCode;
            Created = created;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            TotalPrice = totalPrice;
            Status = status;
            Rooms = rooms;
            AccommodationId = accommodationId;
            CancellationPolicies = cancellationPolicies;
            Cancelled = cancelled;
            IsPackage = isPackage;
        }


        public string ClientReferenceCode { get; }
        public string ReferenceCode { get; }
        public string AccommodationId { get; }
        public DateTime Created { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public MoneyAmount TotalPrice { get; }
        public BookingStatuses Status { get; }
        public List<BookedRoom> Rooms { get; }
        public List<CancellationPolicy> CancellationPolicies { get; }
        public DateTime? Cancelled { get; }
        public bool IsPackage { get; }
    }
}