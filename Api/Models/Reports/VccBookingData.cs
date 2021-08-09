using System;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Models.Reports
{
    public readonly struct VccBookingData
    {
        public string GuestName { get; init; }
        public string ReferenceCode { get; init; }
        public DateTime CheckingDate { get; init; }
        public DateTime CheckOutDate { get; init; }
        public decimal Amount { get; init; }
        public Currencies Currency { get; init; }
        public DateTime CardActivationDate { get; init; }
        public DateTime CardDueDate { get; init; }
        public string CardNumber { get; init; }
        public decimal CardAmount { get; init; }
    }
}