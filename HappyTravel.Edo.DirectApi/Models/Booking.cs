using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Money.Models;

namespace HappyTravel.Edo.DirectApi.Models
{
    public readonly struct Booking
    {
        public Booking(string clientReferenceCode, string referenceCode, DateTime created, DateTime checkInDate, DateTime checkOutDate, DateTime deadlineDate, 
            MoneyAmount totalPrice, BookingStatuses status, List<BookedRoom> rooms, string accommodationId, List<CancellationPolicy> cancellationPolicies, 
            DateTime? cancelled, bool isAdvancePurchaseRate, bool isPackage)
        {
            ClientReferenceCode = clientReferenceCode;
            ReferenceCode = referenceCode;
            Created = created;
            CheckInDate = checkInDate;
            CheckOutDate = checkOutDate;
            DeadlineDate = deadlineDate;
            TotalPrice = totalPrice;
            Status = status;
            Rooms = rooms;
            AccommodationId = accommodationId;
            CancellationPolicies = cancellationPolicies;
            Cancelled = cancelled;
            IsAdvancePurchaseRate = isAdvancePurchaseRate;
            IsPackage = isPackage;
        }


        public string ClientReferenceCode { get; }
        public string ReferenceCode { get; }
        public DateTime Created { get; }
        public DateTime CheckInDate { get; }
        public DateTime CheckOutDate { get; }
        public DateTime DeadlineDate { get; }
        public MoneyAmount TotalPrice { get; }
        public BookingStatuses Status { get; }
        public List<BookedRoom> Rooms { get; }
        public string AccommodationId { get; }
        public List<CancellationPolicy> CancellationPolicies { get; }
        public DateTime? Cancelled { get; }
        public bool IsAdvancePurchaseRate { get; }
        public bool IsPackage { get; }
    }
}