﻿using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Models;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct AccommodationBookingInfo
    {
        [JsonConstructor]
        public AccommodationBookingInfo(int bookingId, AccommodationBookingDetails bookingDetails, int agencyId,
            BookingPaymentStatuses paymentStatus, MoneyAmount totalPrice, MoneyAmount creditCardPrice,
            MoneyAmount cancellationPenalty, string supplier,
            BookingAgentInformation agentInformation, PaymentTypes paymentMethod, List<string> tags,
            bool? isDirectContract, DateTimeOffset? cancellationDate)
        {
            BookingId = bookingId;
            BookingDetails = bookingDetails;
            AgencyId = agencyId;
            PaymentStatus = paymentStatus;
            TotalPrice = totalPrice;
            CreditCardPrice = creditCardPrice;
            CancellationPenalty = cancellationPenalty;
            Supplier = supplier;
            AgentInformation = agentInformation;
            PaymentMethod = paymentMethod;
            Tags = tags;
            IsDirectContract = isDirectContract;
            CancellationDate = cancellationDate;
        }


        public override bool Equals(object obj) => obj is AccommodationBookingInfo other && Equals(other);


        public bool Equals(AccommodationBookingInfo other)
            => Equals((BookingId, BookingDetails, AgencyId, PaymentStatus, TotalPrice, Supplier),
                (other.BookingId, other.BookingDetails, other.AgencyId, other.PaymentStatus, TotalPrice, Supplier));


        public override int GetHashCode() => (BookingId, BookingDetails, AgencyId).GetHashCode();


        public int BookingId { get; }
        public AccommodationBookingDetails BookingDetails { get; }
        public int AgencyId { get; }
        public BookingPaymentStatuses PaymentStatus { get; }
        public MoneyAmount TotalPrice { get; }
        public MoneyAmount CreditCardPrice { get; }
        public MoneyAmount CancellationPenalty { get; }
        public string Supplier { get; }
        public BookingAgentInformation AgentInformation { get; }
        public PaymentTypes PaymentMethod { get; }
        public List<string> Tags { get; }
        public bool? IsDirectContract { get; }
        public DateTimeOffset? CancellationDate { get; }
    }
}