using System;
using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;

namespace HappyTravel.Edo.Api.Models.Reports
{
    public readonly struct AgentWiseReportData
    {
        public string ReferenceCode { get; init; }
        public string AccommodationName { get; init; }
        public PaymentTypes PaymentMethod { get; init; }
        public string GuestName { get; init; }
        public DateTime Created { get; init; }
        public DateTime ArrivalDate { get; init; }
        public DateTime DepartureDate { get; init; }
        public decimal TotalPrice { get; init; }
        public List<BookedRoom> Rooms { get; init; }
        public BookingStatuses Status { get; init; }
    }
}